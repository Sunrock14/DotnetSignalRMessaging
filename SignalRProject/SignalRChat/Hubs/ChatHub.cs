using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRChat.Hubs;

/// <summary>
/// Ana chat hub sınıfı. 
/// Bu sınıf SignalR'ın gerçek zamanlı iletişim yeteneklerini uygular.
/// </summary>
public class ChatHub : Hub
{
    // Kullanıcı bilgilerini saklamak için eşzamanlı sözlük
    private static readonly ConcurrentDictionary<string, UserInfo> _connectedUsers = new();
    // Grup bilgilerini saklamak için eşzamanlı sözlük
    private static readonly ConcurrentDictionary<string, string> _userGroups = new();

    /// <summary>
    /// Kullanıcı bağlantı başlangıcında çağrılır
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        // Bağlantı kimliğini alır
        var connectionId = Context.ConnectionId;

        await Clients.Caller.SendAsync("ReceiveSystemMessage", $"Bağlandınız! Bağlantı ID: {connectionId}");
        await Clients.Others.SendAsync("ReceiveSystemMessage", $"Yeni bir kullanıcı bağlandı!");

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Kullanıcı bağlantı kesildiğinde çağrılır
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        // Kullanıcı bilgisini sözlükten kaldır
        if (_connectedUsers.TryRemove(connectionId, out var userInfo))
        {
            await Clients.All.SendAsync("ReceiveSystemMessage", $"{userInfo.UserName} bağlantısı kesildi.");
            await Clients.All.SendAsync("UpdateUserList", _connectedUsers.Values);
        }

        // Kullanıcıyı gruplardan çıkar
        if (_userGroups.TryRemove(connectionId, out var groupName))
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", "Sistem", $"{userInfo?.UserName} grubu terk etti.");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Kullanıcı kaydı için metod
    /// </summary>
    public async Task Register(string userName)
    {
        var connectionId = Context.ConnectionId;

        // Kullanıcı adının daha önce kullanılıp kullanılmadığını kontrol et
        if (_connectedUsers.Values.Any(u => u.UserName == userName))
        {
            await Clients.Caller.SendAsync(
                "RegistrationFailed", 
                "Bu kullanıcı adı zaten kullanımda.");
            return;
        }

        // Kullanıcı bilgisini sözlüğe ekle
        var userInfo = new UserInfo
        {
            ConnectionId = connectionId,
            UserName = userName,
            IsOnline = true
        };

        _connectedUsers[connectionId] = userInfo;

        // Kullanıcıya başarılı kayıt mesajı gönder
        await Clients.Caller.SendAsync("RegistrationSuccessful", userInfo);

        // Tüm kullanıcılara güncel kullanıcı listesini gönder
        await Clients.All.SendAsync("UpdateUserList", _connectedUsers.Values);

        // Genel kanala katıl
        await JoinGroup("Genel");

        // Tüm kullanıcılara yeni kullanıcı bilgisi gönder
        await Clients.Others.SendAsync("ReceiveSystemMessage", $"{userName} sohbete katıldı!");
    }

    /// <summary>
    /// Genel mesaj gönderme metodu
    /// </summary>
    public async Task SendMessage(string message)
    {
        var connectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(connectionId, out var userInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Mesaj göndermek için önce kaydolmanız gerekiyor.");
            return;
        }

        // Tüm kullanıcılara mesajı gönder
        await Clients.All.SendAsync("ReceiveMessage", userInfo.UserName, message);
    }

    /// <summary>
    /// Özel mesaj gönderme metodu
    /// </summary>
    public async Task SendPrivateMessage(string receiverConnectionId, string message)
    {
        var senderConnectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(senderConnectionId, out var senderInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Mesaj göndermek için önce kaydolmanız gerekiyor.");
            return;
        }

        // Alıcı kullanıcı bilgisini kontrol et
        if (!_connectedUsers.TryGetValue(receiverConnectionId, out var receiverInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Alıcı kullanıcı artık bağlı değil.");
            return;
        }

        // Alıcıya özel mesaj gönder
        await Clients.Client(receiverConnectionId).SendAsync("ReceivePrivateMessage", senderInfo.UserName, message, senderConnectionId);

        // Gönderene de mesajı göster
        await Clients.Caller.SendAsync("ReceivePrivateMessage", senderInfo.UserName, message, receiverConnectionId);
    }

    /// <summary>
    /// Gruba katılma metodu
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        var connectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(connectionId, out var userInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Gruba katılmak için önce kaydolmanız gerekiyor.");
            return;
        }

        // Kullanıcıyı eski gruptan çıkar
        if (_userGroups.TryRemove(connectionId, out var oldGroupName))
        {
            await Groups.RemoveFromGroupAsync(connectionId, oldGroupName);
            await Clients.Group(oldGroupName).SendAsync("ReceiveGroupMessage", "Sistem", $"{userInfo.UserName} grubu terk etti.");
        }

        // Kullanıcıyı yeni gruba ekle
        await Groups.AddToGroupAsync(connectionId, groupName);
        _userGroups[connectionId] = groupName;

        // Grup mesajı gönder
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", "Sistem", $"{userInfo.UserName} gruba katıldı.");
        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Grup mesajı gönderme metodu
    /// </summary>
    public async Task SendGroupMessage(string message)
    {
        var connectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(connectionId, out var userInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Mesaj göndermek için önce kaydolmanız gerekiyor.");
            return;
        }

        // Kullanıcının bir grupta olup olmadığını kontrol et
        if (!_userGroups.TryGetValue(connectionId, out var groupName))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Herhangi bir gruba katılmadınız.");
            return;
        }

        // Grup mesajı gönder
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", userInfo.UserName, message);
    }

    /// <summary>
    /// Kullanıcının yazma durumunu bildiren metod
    /// </summary>
    public async Task NotifyTyping(bool isTyping)
    {
        var connectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(connectionId, out var userInfo))
        {
            return;
        }

        // Kullanıcının bir grupta olup olmadığını kontrol et
        if (_userGroups.TryGetValue(connectionId, out var groupName))
        {
            // Gruptaki diğer kullanıcılara yazma durumu bildir
            await Clients.GroupExcept(groupName, connectionId).SendAsync("UserTyping", userInfo.UserName, isTyping);
        }
        else
        {
            // Tüm kullanıcılara yazma durumu bildir
            await Clients.Others.SendAsync("UserTyping", userInfo.UserName, isTyping);
        }
    }

    /// <summary>
    /// Dosya paylaşma metodu (temel versiyon - base64 ile)
    /// </summary>
    public async Task ShareFile(string fileName, string base64Content)
    {
        var connectionId = Context.ConnectionId;

        if (!_connectedUsers.TryGetValue(connectionId, out var userInfo))
        {
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "Dosya paylaşmak için önce kaydolmanız gerekiyor.");
            return;
        }

        // Kullanıcının bir grupta olup olmadığını kontrol et
        if (_userGroups.TryGetValue(connectionId, out var groupName))
        {
            // Gruptaki kullanıcılara dosya bilgisini gönder
            await Clients.Group(groupName).SendAsync("ReceiveFile", userInfo.UserName, fileName, base64Content);
        }
        else
        {
            // Tüm kullanıcılara dosya bilgisini gönder
            await Clients.All.SendAsync("ReceiveFile", userInfo.UserName, fileName, base64Content);
        }
    }
}

/// <summary>
/// Kullanıcı bilgilerini tutan sınıf
/// </summary>
public class UserInfo
{
    public string ConnectionId { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool IsOnline { get; set; }
}
