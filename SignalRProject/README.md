# SignalR Chat Uygulaması

Bu proje, .NET 9 ve React 19 kullanarak geliştirilen, SignalR'ın tüm özelliklerini gösteren gerçek zamanlı bir sohbet uygulamasıdır.

## Özellikler

- Kullanıcı kaydı ve oturum açma
- Genel sohbet
- Özel mesajlaşma
- Grup sohbeti
- Kullanıcı çevrimiçi durumu
- Yazıyor bildirimleri
- Dosya paylaşımı
- Otomatik yeniden bağlanma

## SignalR Özellikleri

Bu projede kullanılan SignalR özellikleri:

- Hub mimarisi
- Bağlantı yönetimi
- Gruplar ve kullanıcı yönetimi
- Yayın (broadcast) işlemleri
- Özel istemcilere mesaj gönderme
- Bağlantı durumu değişikliklerini izleme
- Otomatik yeniden bağlanma
- Yetkilendirme ve güvenlik

## Teknolojiler

### Backend
- .NET 9
- ASP.NET Core
- SignalR

### Frontend
- React 19
- @microsoft/signalr paketi

## Başlangıç

### Backend'i Çalıştırma

```bash
cd SignalRProject/SignalRChat
dotnet run
```

### Frontend'i Çalıştırma

```bash
cd SignalRProject/Frontend/chat-client
npm start
```

## Kullanım Kılavuzu

1. Uygulamayı başlatın
2. Kullanıcı adınızı girerek kaydolun
3. Genel sohbet odasında mesajlaşmaya başlayın
4. Yan panelden bir kullanıcıya tıklayarak özel mesaj gönderebilirsiniz
5. Grup adı girerek yeni bir gruba katılabilirsiniz
6. Dosya eki göndermek için ataç simgesini kullanabilirsiniz

## Proje Yapısı

```
SignalRProject/
├── SignalRChat/              # Backend (.NET 9)
│   ├── Program.cs            # ASP.NET Core yapılandırması ve başlatma
│   ├── Hubs/                 # SignalR Hub'ları
│   │   └── ChatHub.cs        # Ana chat hub sınıfı
│   └── ...
└── Frontend/                 # Frontend (React 19)
    └── chat-client/
        ├── src/
        │   ├── App.js        # Ana uygulama bileşeni
        │   ├── App.css       # Stil dosyası
        │   └── services/
        │       └── signalrService.js # SignalR istemci yapılandırması
        └── ...
```

## SignalR Hakkında

SignalR, Microsoft tarafından geliştirilen ve real-time web uygulamaları oluşturmak için kullanılan bir kütüphanedir. WebSocket teknolojisini kullanarak sunucu ve istemci arasında çift yönlü iletişim sağlar. Bağlantı türü olarak WebSocket, Server-Sent Events, ForeverFrame ve Long Polling gibi teknikleri kullanır ve uygun bağlantı türünü otomatik olarak seçer.

### SignalR'ın Temel Kavramları

1. **Hub**: Sunucu ve istemci arasındaki iletişimi sağlayan merkezi bileşendir. İstemcilerin çağırabileceği metotları ve sunucunun istemcilerde çağırabileceği metotları tanımlar.

2. **Bağlantı**: Her istemci için oluşturulan benzersiz bir kimliktir. Bu kimlik üzerinden istemcilere özel mesajlar gönderilebilir.

3. **Gruplar**: İstemcilerin mantıksal gruplandırılmasıdır. Bir gruba mesaj göndermek, gruptaki tüm istemcilere mesaj göndermek anlamına gelir.

4. **Yayın (Broadcast)**: Tüm bağlı istemcilere mesaj göndermek için kullanılır.

## Katkıda Bulunma

1. Bu depoyu fork edin
2. Özellik dalınızı oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some amazing feature'`)
4. Dalınıza push edin (`git push origin feature/amazing-feature`)
5. Bir Pull Request açın 