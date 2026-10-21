# ComfyUI API Gateway

**ComfyUI için ASP.NET Core tabanlı REST API katmanı**

## 1. Proje Hakkında

ComfyUI API Gateway, ComfyUI'nin görüntü üretme ve görüntü düzenleme iş akışlarını HTTP API üzerinden kullanılabilir hâle getiren bir ASP.NET Core uygulamasıdır.

Uygulama, ComfyUI'nin mevcut API'siyle iletişim kurar ve önceden tanımlanmış iş akışlarını daha sade endpoint'ler üzerinden çalıştırır. Böylece ComfyUI'nin dahili prompt yapısını istemci uygulamalarına doğrudan açmadan metinden görüntü üretme ve görüntüden görüntü oluşturma işlemleri sunulabilir.

Proje ayrıca görüntü üretim durumlarının WebSocket üzerinden takip edilmesine ve SignalR aracılığıyla istemcilere aktarılmasına yönelik bir yapı içerir.

## 2. Temel Özellikler

- **Metinden görüntü üretimi:** Tanımlı bir iş akışına pozitif ve negatif prompt gönderilmesi.
- **Görüntüden görüntü üretimi:** Kaynak görüntü ve maske dosyalarının yüklenerek bir iş akışına aktarılması.
- **İş akışı seçimi:** Metinden görüntü, görüntüden görüntü kategorilerindeki mevcut şemaların listelenmesi.
- **Görüntü boyutlandırma:** Yapılandırılmış sınırları aşan görüntülerin yeniden boyutlandırılması.
- **Üretim çıktısına erişim:** Tamamlanmış bir üretimin görüntüsünün API üzerinden alınması.
- **Gerçek zamanlı bildirimler:** ComfyUI WebSocket mesajlarının SignalR hub üzerinden istemcilere iletilmesi.

## 3. Teknoloji Altyapısı

| Teknoloji | Kullanım amacı |
|---|---|
| C# / .NET 8 | Uygulama altyapısı |
| ASP.NET Core Minimal API | HTTP endpoint'lerinin yayınlanması |
| Flurl ve Flurl.Http | ComfyUI projesine HTTP istekleri göndermek için kullanılmaktadır |
| Magick.NET | Görüntülerin işlenmesi ve yeniden boyutlandırılması |
| SignalR | İstemcilere websocket üzerinden gerçek zamanlı bildirimler |
| System.Net.WebSockets | ComfyUI ile WebSocket bağlantısının sağlanması |

Proje dosyası .NET 8 hedefini belirtir. Linux için Docker yapılandırmasına yönelik ayarlar da mevcuttur.

## 4. Mimari

```text
İstemci uygulaması
       |
       | HTTP / REST
       v
ASP.NET Core API Gateway
       |
       +---- Prompt şablonları
       |
       +---- Görüntü işleme servisi
       |
       +---- Prompt REST servisi
       |            |
       |            v
       |         ComfyUI API
       |            |
       |            v
       |         Üretim çıktısı
       |
       +---- WebSocket dinleyicisi
                    |
                    v
               SignalR Hub
                    |
                    v
             İstemci bildirimleri
```

### Bileşenler

**ComfyUiEndpoints**

REST endpoint'lerini tanımlar. Şemaları listeler, istek parametrelerini prompt şablonlarına yerleştirir ve üretim isteğini ilgili servise iletir.

**PromptRestService**

ComfyUI ile HTTP iletişimini yürütür. Prompt gönderimi, görüntü yükleme, üretim geçmişini sorgulama ve çıktı dosyasını alma işlemlerini gerçekleştirir.

**ImageProcessService**

Gelen görüntüleri Magick.NET ile işler. Yeniden boyutlandırma ayarları etkinse belirtilen boyut sınırlarını uygular ve görüntü metaverilerini temizler.

**ComfyUISocketListener**

ComfyUI WebSocket bağlantısından gelen mesajları dinler. İlerleme, durum, kesinti ve başarılı yürütme gibi olayları SignalR üzerinden yayınlar.

**ComfyUiHub**

İstemcilerin SignalR aracılığıyla gerçek zamanlı bildirim alabilmesini sağlayan hub bileşenidir.

## 5. API Referansı


| HTTP | Endpoint | Açıklama |
|---|---|---|
| GET | `/api/v1/comfy-ui/text-to-image/schemes` | Metinden görüntü şemalarını listeler. |
| POST | `/api/v1/comfy-ui/text-to-image/schemes/{scheme}/generate` | Seçilen şemayla görüntü üretimi başlatır. |
| GET | `/api/v1/comfy-ui/image-to-image/schemes` | Görüntüden görüntü şemalarını listeler. |
| POST | `/api/v1/comfy-ui/image-to-image/schemes/{scheme}/generate` | Kaynak görüntü ve maskeyle üretim başlatır. |
| GET | `/api/v1/comfy-ui/images/{promptId}/view` | Üretim çıktısını PNG olarak döndürür. |

### 5.1. Metinden Görüntü Üretimi

```http
POST /api/v1/comfy-ui/text-to-image/schemes/{scheme}/generate
Content-Type: application/json
```

Örnek istek gövdesi:

```json
{
  "positive": "A cinematic landscape at sunset",
  "negative": "blurry, low quality"
}
```

İşlem akışı:

1. İlgili şema yüklenir.
2. Pozitif ve negatif prompt alanları doldurulur.
3. İstemci kimliği, rastgele seed ve benzersiz kimlik gibi değerler şablona yerleştirilir.
4. Prompt JSON'a dönüştürülür.
5. ComfyUI'nin `/prompt` endpoint'ine gönderilir.
6. ComfyUI yanıtı istemciye döndürülür.

Şema bulunamazsa `404 Not Found`, prompt JSON'u oluşturulamazsa `400 Bad Request` döndürülür. ComfyUI isteğinin başarısız olması durumunda mevcut servis bir hata döndürür.

### 5.2. Görüntüden Görüntü Üretimi

```http
POST /api/v1/comfy-ui/image-to-image/schemes/{scheme}/generate
Content-Type: multipart/form-data
```

Beklenen form alanları:

| Alan | Tür | Açıklama |
|---|---|---|
| `image` | Dosya | Kaynak görüntü |
| `mask` | Dosya | Maske görüntüsü |
| `positive` | Metin | Pozitif prompt |
| `negative` | Metin | Negatif prompt |

İşlem sırasında kaynak görüntü ve maske işlenir, ComfyUI'ye yüklenir ve dönen dosya adları prompt şablonuna eklenir. Ardından üretim isteği gönderilir.

### 5.3. Üretilen Görüntüyü Alma

```http
GET /api/v1/comfy-ui/images/{promptId}/view
```

Gateway, `promptId` üzerinden ComfyUI üretim geçmişini sorgular, çıktı dosyasının adını bulur ve görüntüyü `image/png` içerik türüyle döndürür.

Çıktı bulunamazsa `404 Not Found` yanıtı döndürülür.

Mevcut uygulamada birden fazla çıktı görüntüsü bulunması durumunda yalnızca ilk uygun çıktı dosyası seçilir.

## 6. Gerçek Zamanlı İletişim

SignalR hub adresi:

```text
/hubs/comfy-ui
```

WebSocket dinleyicisi tarafından aktarılan başlıca olaylar şunlardır:

| Olay | Açıklama |
|---|---|
| `ProgressMessage` | Üretim ilerlemesi |
| `StatusMessage` | Kuyruk veya durum bilgisi |
| `Interrupted` | İşlemin kesilmesi |
| `ExecutedMessage` | Başarılı yürütme bildirimi |


## 7. Kurulum ve Çalıştırma

### Gereksinimler

- .NET 8 SDK
- Çalışır durumda ComfyUI kurulumu
- Gerekli modeller ve custom node'lar
- Projede tanımlanan prompt şablonları

### Adımlar

1. Depoyu klonlayın:

   ```bash
   git clone https://github.com/enescakr55/comfyui-api-gateway.git
   cd comfyui-api-gateway
   ```

2. `appsettings.json` dosyasını düzenleyin. ComfyUI adresini, portunu ve prompt klasörünün yolunu tanımlayın.

3. ComfyUI'yi çalıştırın

4. Uygulamayı başlatın:

   ```bash
   dotnet restore
   dotnet run --project ApiService
   ```

5. Artık API Gateway üzerinden görüntü oluşturmaya hazırsınız.

### Uygulamanın test edildiği kurulum

ComfyUI v0.3.44

ComfyUI_frontend v1.23.4

ComfyUI_desktop v0.4.55


Üçüncü Taraf Yazılım: Bu proje, ComfyUI ile API'si üzerinden iletişim kurar. ComfyUI, GPL-3.0 lisansı altında lisanslanmış ayrı bir projedir. Bu proje, ComfyUI'nin veya kaynak kodunun mülkiyetinin kendisine ait olduğunu iddia etmez. ComfyUI'nin lisansı ve kullanım koşulları için ComfyUI deposunu inceleyebilirsiniz.
