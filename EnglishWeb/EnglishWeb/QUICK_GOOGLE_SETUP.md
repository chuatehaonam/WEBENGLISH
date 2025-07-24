# Fix Lỗi Google OAuth "invalid_client" - Hướng dẫn nhanh

## ⚠️ LỖI: The OAuth client was not found - Error 401: invalid_client

Lỗi này có nghĩa là bạn chưa cấu hình Google OAuth credentials.

## ✅ GIẢI PHÁP (5 phút):

### 1. Tạo Google OAuth App
1. Vào: https://console.cloud.google.com/apis/credentials
2. Tạo project mới (nếu chưa có)
3. Click **+ CREATE CREDENTIALS** → **OAuth 2.0 Client IDs**
4. Chọn **Web application**
5. Thêm **Authorized redirect URIs**:
   ```
   http://localhost:YOUR_PORT/User/GoogleCallback
   https://localhost:YOUR_PORT/User/GoogleCallback
   ```

### 2. Lấy thông tin credentials
Sau khi tạo, bạn sẽ có:
- **Client ID**: `123456789-abc.apps.googleusercontent.com`
- **Client Secret**: `GOCSPX-xyz123abc456`

### 3. Cập nhật Web.config
Mở file `Web.config`, tìm và thay thế:

```xml
<!-- TỪ: -->
<add key="GoogleClientId" value="PASTE_YOUR_REAL_CLIENT_ID_HERE" />
<add key="GoogleClientSecret" value="PASTE_YOUR_REAL_CLIENT_SECRET_HERE" />

<!-- THÀNH: -->
<add key="GoogleClientId" value="123456789-abc.apps.googleusercontent.com" />
<add key="GoogleClientSecret" value="GOCSPX-xyz123abc456" />
```

### 4. Tìm port của ứng dụng
- Chạy ứng dụng (F5)
- Xem URL: `https://localhost:44300/`
- Port = `44300`

### 5. Cập nhật redirect URI
Quay lại Google Console, cập nhật redirect URI với port đúng:
```
https://localhost:44300/User/GoogleCallback
```

### 6. Test lại
- Build và chạy ứng dụng
- Click "Đăng nhập bằng Google"
- Phải redirect đến Google login page

## 🔧 Troubleshooting

| Lỗi | Nguyên nhân | Giải pháp |
|-----|-------------|-----------|
| `invalid_client` | Client ID sai hoặc chưa setup | Kiểm tra Web.config |
| `redirect_uri_mismatch` | URL callback không khớp | Cập nhật redirect URI |
| `access_denied` | User từ chối permission | Bình thường, user có thể thử lại |

## 📞 Cần hỗ trợ?
Nếu vẫn lỗi, check:
1. Google Console project có đúng không?
2. APIs & Services → Library → Google+ API đã enable chưa?
3. Web.config đã save chưa?
4. Application đã restart chưa? 