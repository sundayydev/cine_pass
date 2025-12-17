# Actor Image Upload Feature

## Tổng quan

Đã implement chức năng upload ảnh cho diễn viên (actors), tương tự như logic upload ảnh ở movies. User có thể:
- Upload file ảnh từ máy tính
- Paste URL ảnh từ internet
- Preview ảnh trước khi lưu
- Xóa hoặc thay đổi ảnh

## Các thay đổi

### 1. CreateActor.tsx

**Thêm states:**
- `imageFile`: Lưu file ảnh được chọn
- `imagePreview`: Lưu preview URL của ảnh

**Logic upload:**
1. Tạo actor TRƯỚC (không có imageUrl từ file)
2. Upload ảnh với actorId (nếu có file)
3. Cập nhật imageUrl cho actor sau khi upload thành công

**UI Features:**
- File input với validation (10MB max, JPG/PNG/GIF/WEBP)
- Preview ảnh với hover actions (Thay đổi, Xóa)
- Hoặc nhập URL ảnh trực tiếp
- Divider "Hoặc" giữa 2 options

### 2. EditActor.tsx

**Thêm states:**
- `imageFile`: Lưu file ảnh mới được chọn
- `imagePreview`: Lưu preview URL của ảnh mới

**Logic upload:**
1. Cập nhật thông tin actor (không bao gồm ảnh upload nếu có file)
2. Upload ảnh mới nếu có file
3. Cập nhật imageUrl cho actor với URL mới

**UI Features:**
- Hiển thị ảnh hiện tại (nếu có)
- Cho phép xóa ảnh hiện tại
- Upload ảnh mới với preview
- Hoặc nhập URL ảnh mới

## API Integration

Sử dụng `uploadApi.uploadEntityImage()` từ `@/services/apiUpload`:
```typescript
const uploadResult = await uploadApi.uploadEntityImage(
    imageFile,      // File object
    "actor",        // Entity type
    actorId         // Actor ID
);
```

## Validation

- **File size**: Tối đa 10MB
- **File types**: JPG, JPEG, PNG, GIF, WEBP
- **Error handling**: Toast notifications cho user

## User Experience

### CreateActor:
1. User chọn ảnh hoặc nhập URL
2. Preview ngay lập tức
3. Submit form → Tạo actor → Upload ảnh → Cập nhật imageUrl
4. Nếu lỗi ở bước upload, actor vẫn được tạo, user có thể edit sau

### EditActor:
1. Hiển thị ảnh hiện tại (nếu có)
2. User có thể:
   - Giữ ảnh cũ (không làm gì)
   - Xóa ảnh cũ
   - Upload ảnh mới
   - Paste URL mới
3. Submit form → Cập nhật info → Upload ảnh mới (nếu có) → Cập nhật imageUrl

## Notes

- Tất cả các icons (`Upload`, `X`, `ImageIcon`) và utility `cn` đều được sử dụng trong UI components
- Lint warnings về "declared but never read" là false positives - các variables này được sử dụng trong JSX
- Logic upload tương tự hoàn toàn với movie upload để consistency
