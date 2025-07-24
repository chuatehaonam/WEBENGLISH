# Hướng dẫn Setup API Key An toàn

## Để chạy ứng dụng:

1. **Tạo file `appsettings.json`** từ file mẫu:
   ```bash
   copy appsettings.example.json appsettings.json
   ```

2. **Thay thế API key** trong file `appsettings.json`:
   ```json
   {
     "ApiSettings": {
       "OpenRouterApiKey": "your-actual-api-key-here",
       "OpenRouterBaseUrl": "https://openrouter.ai/api/v1"
     }
   }
   ```

3. **Lấy API key** từ [OpenRouter](https://openrouter.ai/)

## Bảo mật:

- ✅ File `appsettings.json` đã được thêm vào `.gitignore`
- ✅ API key không bao giờ được commit lên GitHub
- ✅ Chỉ file `appsettings.example.json` (không chứa API key thật) được commit

## Lưu ý quan trọng:

- **KHÔNG BAO GIỜ** commit file `appsettings.json` lên GitHub
- Nếu cần deploy production, sử dụng environment variables:
  ```
  OPENROUTER_API_KEY=your-api-key
  ```

## Kiểm tra:

Chạy lệnh sau để đảm bảo file config được ignore:
```bash
git status
```

File `appsettings.json` không được hiển thị trong danh sách files thay đổi. 