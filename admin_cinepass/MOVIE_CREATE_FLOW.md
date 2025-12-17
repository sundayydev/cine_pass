# Flow Tạo Phim Mới - Với Upload Poster

## Thay đổi

Đã cập nhật flow tạo phim trong `CreateMovie.tsx` để:

1. **Tạo phim TRƯỚC** (có ID sẵn)
2. **Upload poster** với movieId vào folder `cinepass/movie/{movieId}`
3. **Cập nhật posterUrl** ngược lại cho phim

## Lý do thay đổi

### Trước (Cách cũ):
```
User chọn ảnh → Upload ngay → Lấy URL → Submit form → Tạo phim với posterUrl
```

**Vấn đề:**
- Upload ảnh vào folder generic (không có movieId)
- Không tổ chức được ảnh theo từng phim
- Nếu tạo phim thất bại, ảnh đã upload vẫn tồn tại (lãng phí)

### Sau (Cách mới):
```
User chọn ảnh → Lưu file tạm → Submit form → Tạo phim → Upload ảnh với movieId → Update posterUrl
```

**Ưu điểm:**
- ✅ Ảnh được organize vào folder: `cinepass/movie/{movieId}/`
- ✅ Dễ quản lý ảnh theo từng phim
- ✅ Không upload nếu tạo phim thất bại
- ✅ URL có structure: `/movie/movie-123/poster.jpg`

## Chi tiết Implementation

### 1. State Management

```tsx
const [posterFile, setPosterFile] = useState<File | null>(null);
```

- Lưu file ảnh tạm thời trong state
- Không upload ngay khi user chọn file

### 2. File Selection UI

```tsx
<input
  type="file"
  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
  onChange={(e) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file (size, type)
      setPosterFile(file); // Lưu file
      
      // Create preview locally
      const reader = new FileReader();
      reader.onloadend = () => {
        field.onChange(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }}
/>
```

### 3. Submit Flow

```tsx
const onSubmit = async (values: FormValues) => {
  let createdMovieId: string | null = null;

  try {
    // BƯỚC 1: Tạo phim (không có posterUrl)
    toast.info("Đang tạo phim...");
    const createdMovie = await movieApi.create({
      title: values.title,
      // ... other fields
      // Không gửi posterUrl
    });
    createdMovieId = createdMovie.id;

    // BƯỚC 2: Upload poster (nếu có) với movieId
    if (posterFile && createdMovieId) {
      toast.info("Đang upload poster...");
      
      const { uploadApi } = await import("@/services/apiUpload");
      const uploadResult = await uploadApi.uploadEntityImage(
        posterFile,
        "movie",
        createdMovieId // ← Quan trọng: folder organization
      );

      // BƯỚC 3: Update posterUrl cho phim
      toast.info("Đang cập nhật poster...");
      await movieApi.update(createdMovieId, {
        posterUrl: uploadResult.secureUrl,
      });
    }

    toast.success("Đã tạo phim mới thành công!");
    navigate(PATHS.MOVIES);
  } catch (error) {
    // Error handling với recovery logic
    if (createdMovieId) {
      // Phim đã tạo nhưng upload/update lỗi
      toast.warning(
        "Phim đã được tạo nhưng có lỗi khi upload poster. Bạn có thể chỉnh sửa phim để thêm poster sau.",
        { duration: 7000 }
      );
      navigate(PATHS.MOVIES); // Vẫn chuyển trang
      return;
    }
    // Xử lý lỗi tạo phim
  }
};
```

### 4. Error Handling

#### Scenario 1: Lỗi tạo phim
```
→ Không upload gì cả
→ Hiển thị error message
→ User vẫn ở trang CreateMovie
```

#### Scenario 2: Tạo phim OK, lỗi upload/update
```
→ Phim đã được tạo thành công
→ Hiển thị warning: "Phim đã tạo nhưng lỗi upload poster"
→ Chuyển về trang danh sách phim
→ User có thể edit phim để thêm poster sau
```

## Backend Endpoints Sử dụng

### 1. Create Movie
```
POST /api/movies
Body: MovieCreateDto (không có posterUrl)
Response: { id, title, ... }
```

### 2. Upload Entity Image
```
POST /api/upload/entity/movie?entityId={movieId}
Content-Type: multipart/form-data
Body: file
Response: { publicId, secureUrl, ... }
```

Ảnh sẽ được upload vào: `cinepass/movie/{movieId}/filename.jpg`

### 3. Update Movie
```
PUT /api/movies/{id}
Body: { posterUrl: "https://..." }
Response: Updated movie
```

## Folder Structure trên Cloudinary

```
cinepass/
└── movie/
    ├── {movie-id-1}/
    │   ├── poster.jpg
    │   ├── banner.jpg
    │   └── gallery/
    │       ├── img1.jpg
    │       └── img2.jpg
    ├── {movie-id-2}/
    │   └── poster.jpg
    └── {movie-id-3}/
        └── poster.jpg
```

## UI Changes

### Before:
```
┌─────────────────────┐
│  Upload Component   │  ← Upload ngay khi chọn
│  (with loading...)  │
└─────────────────────┘
```

### After:
```
┌─────────────────────┐
│  File Preview       │  ← Chỉ preview local
│  (local base64)     │
│                     │
│  "Ảnh sẽ được       │  ← Thông báo rõ ràng
│   upload sau khi    │
│   tạo phim"         │
└─────────────────────┘
```

## User Experience

1. **User chọn ảnh**
   - Thấy preview ngay lập tức
   - Không cần chờ upload
   - Có thể thay đổi/xóa ảnh bao nhiêu lần cũng được

2. **User submit form**
   - Toast: "Đang tạo phim..."
   - Toast: "Đang upload poster..."
   - Toast: "Đang cập nhật poster..."
   - Toast: "Đã tạo phim mới thành công!"

3. **Nếu có lỗi**
   - Lỗi validate → Hiển thị error tại field
   - Lỗi tạo phim → Hiển thị toast error, giữ nguyên form
   - Lỗi upload → Warning toast, vẫn chuyển trang (phim đã tạo)

## Testing Checklist

- [ ] Tạo phim không có poster → OK
- [ ] Tạo phim có poster → OK, poster upload vào đúng folder
- [ ] Validation file size (>10MB) → Error toast
- [ ] Validation file type (không phải ảnh) → Error toast
- [ ] Network error khi tạo phim → Error toast, không upload
- [ ] Network error khi upload → Warning toast, phim vẫn tạo
- [ ] Thay đổi ảnh nhiều lần trước khi submit → OK
- [ ] Xóa ảnh và submit → OK (tạo phim không có poster)

## Migration Notes

### Phim đã tồn tại (trước khi update)
- Posterà ở folder generic hoặc không có folder structure
- **Không cần migrate**, giữ nguyên posterUrl

### Phim mới (sau khi update)
- Poster sẽ được organize theo movieId
- Structure rõ ràng, dễ quản lý

## Future Enhancements

1. **Multiple images**
   - Thêm gallery images (nhiều ảnh)
   - Upload tất cả với cùng movieId

2. **Crop/Resize trước upload**
   - Preview với crop tool
   - Upload ảnh đã crop

3. **Progress indicator**
   - Hiển thị % upload
   - Cancel upload giữa chừng

4. **Retry logic**
   - Retry tự động nếu upload fail
   - Manual retry button

5. **Drag & drop**
   - Support drag & drop file
   - Multiple files selection
