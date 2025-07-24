# Hướng dẫn cài đặt Google OAuth cho EnglishWeb

## Bước 1: Tạo Google OAuth Credentials

1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project hiện có
3. Bật Google+ API và Google OAuth2 API:
   - Vào **APIs & Services** > **Library**
   - Tìm và bật **Google+ API**
   - Tìm và bật **Google OAuth2 API**

4. Tạo OAuth 2.0 Credentials:
   - Vào **APIs & Services** > **Credentials**
   - Click **Create Credentials** > **OAuth 2.0 Client IDs**
   - Chọn **Web application**
   - Đặt tên: "EnglishWeb Login"
   
5. Cấu hình Authorized redirect URIs:
   ```
   http://localhost:port/User/GoogleCallback
   https://yourdomain.com/User/GoogleCallback
   ```
   (Thay `port` bằng port của ứng dụng, ví dụ: 44300)

6. Lưu **Client ID** và **Client Secret**

## Bước 2: Cập nhật Web.config

Mở file `Web.config` và thay thế:

```xml
<add key="GoogleClientId" value="YOUR_GOOGLE_CLIENT_ID" />
<add key="GoogleClientSecret" value="YOUR_GOOGLE_CLIENT_SECRET" />
```

Bằng thông tin thực tế từ Google Console:

```xml
<add key="GoogleClientId" value="123456789-abc.apps.googleusercontent.com" />
<add key="GoogleClientSecret" value="abcd1234-xyz890" />
```

## Bước 3: Test chức năng

1. Build và chạy ứng dụng
2. Truy cập trang Login
3. Click "Đăng nhập bằng Google"
4. Đăng nhập bằng tài khoản Google
5. Kiểm tra trong database xem user mới có được tạo không

## Lưu ý bảo mật

- **KHÔNG** commit Client Secret vào Git
- Sử dụng environment variables hoặc Azure Key Vault cho production
- Cấu hình HTTPS cho production

## Cách hoạt động

1. **User mới**: Khi đăng nhập bằng Google lần đầu, hệ thống tự động tạo tài khoản mới với:
   - `FullName`: Lấy từ Google profile
   - `Email`: Lấy từ Google account
   - `PasswordHash`: "GOOGLE_AUTH" (đánh dấu là tài khoản Google)

2. **User cũ**: Nếu email đã tồn tại, đăng nhập trực tiếp

3. **Session**: Sau khi đăng nhập thành công, user được lưu vào Session giống như đăng nhập thông thường

## Troubleshooting

### Lỗi "redirect_uri_mismatch"
- Kiểm tra URL callback trong Google Console có đúng không
- Đảm bảo protocol (http/https) và port khớp nhau

### Lỗi "invalid_client"
- Kiểm tra Client ID và Client Secret
- Đảm bảo đã bật APIs cần thiết

### User không được tạo
- Kiểm tra database connection
- Xem log lỗi trong Visual Studio Output 