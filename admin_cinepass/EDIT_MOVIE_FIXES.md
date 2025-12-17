# EditMovie Component - Fixes & Updates

## Các vấn đề đã sửa

### 1. ✅ Lỗi Select Status & Category không hiển thị đúng

**Nguyên nhân:**
- Backend trả về status/category dạng **PascalCase**: `"Showing"`, `"Action"`, `"Comedy"`, etc.
- Form cần status/category dạng **UPPERCASE**: `"NOW_SHOWING"`, `"ACTION"`, `"COMEDY"`, etc.
- Khi load movie, form nhận giá trị PascalCase nhưng Select không match với options (UPPERCASE) → Không hiển thị được

**Giải pháp:**
```tsx
// Map backend status (PascalCase) to form status (UPPERCASE)
const mapBackendStatusToForm = (status: string): FormValues['status'] => {
  const statusMap: Record<string, FormValues['status']> = {
    'ComingSoon': 'COMING_SOON',
    'Showing': 'NOW_SHOWING',
    'Ended': 'ENDED',
  };
  return statusMap[status] || 'COMING_SOON';
};

// Map backend category (PascalCase) to form category (UPPERCASE)
const mapBackendCategoryToForm = (category: string): FormValues['category'] => {
  const categoryMap: Record<string, FormValues['category']> = {
    'Movie': 'MOVIE',
    'Series': 'SERIES',
    'Documentary': 'DOCUMENTARY',
    'Animation': 'ANIMATION',
    'Action': 'ACTION',
    'Comedy': 'COMEDY',
    // ... etc
  };
  return categoryMap[category] || 'MOVIE';
};

// Sử dụng khi load movie
form.reset({
  // ...
  status: mapBackendStatusToForm(movie.status),
  category: mapBackendCategoryToForm(movie.category),
});
```

### 2. ✅ Thêm chức năng Upload Poster

**Tính năng mới:**
- Hiển thị poster hiện tại của phim (nếu có)
- Cho phép thay đổi poster
- Preview ảnh mới trước khi upload
- Upload ảnh vào folder `cinepass/movie/{movieId}/`
- Validation file size (max 10MB) và file type (JPG, PNG, GIF, WEBP)

**Flow:**
```
1. Load movie → Hiển thị poster hiện tại (nếu có)
2. User chọn ảnh mới → Preview local
3. User submit form:
   a. Upload poster mới (nếu có) → Lấy URL
   b. Cập nhật movie với posterUrl mới
```

**State management:**
```tsx
const [posterFile, setPosterFile] = useState<File | null>(null);
const [currentPosterUrl, setCurrentPosterUrl] = useState<string>("");
```

**Submit logic:**
```tsx
// BƯỚC 1: Upload poster mới (nếu có)
let newPosterUrl = values.posterUrl || currentPosterUrl;
if (posterFile) {
  try {
    toast.info("Đang upload poster...");
    const { uploadApi } = await import("@/services/apiUpload");
    const uploadResult = await uploadApi.uploadEntityImage(
      posterFile,
      "movie",
      id
    );
    newPosterUrl = uploadResult.secureUrl;
  } catch (uploadError) {
    toast.error("Lỗi khi upload poster, sẽ giữ nguyên poster cũ");
  }
}

// BƯỚC 2: Cập nhật movie với posterUrl mới
const payload: MovieUpdateDto = {
  // ...
  posterUrl: newPosterUrl || undefined,
};
```

## UI Changes

### Before:
```
┌─────────────────────────────┐
│  Poster Phim                │
│  ┌───────────────────────┐  │
│  │ Chức năng upload      │  │
│  │ đang phát triển       │  │
│  └───────────────────────┘  │
└─────────────────────────────┘
```

### After:

**Khi chưa có poster:**
```
┌─────────────────────────────┐
│  Poster Phim                │
│  ┌───────────────────────┐  │
│  │   [ImageIcon]         │  │
│  │   Click để chọn ảnh   │  │
│  │   PNG, JPG (max 10MB) │  │
│  └───────────────────────┘  │
│  Poster hiện tại của phim   │
└─────────────────────────────┘
```

**Khi đã có poster:**
```
┌─────────────────────────────┐
│  Poster Phim                │
│  ┌───────────────────────┐  │
│  │                       │  │
│  │   [Poster Image]      │  │ ← Hover để hiện buttons
│  │                       │  │
│  │   [Thay đổi] [Xóa]    │  │ ← Hiện khi hover
│  └───────────────────────┘  │
│  Ảnh mới sẽ được upload    │
│  khi lưu                    │
└─────────────────────────────┘
```

## Error Handling

### Upload poster thất bại:
```tsx
catch (uploadError) {
  console.error("Poster upload error:", uploadError);
  toast.error("Lỗi khi upload poster, sẽ giữ nguyên poster cũ");
}
```
- Form vẫn submit thành công
- Giữ nguyên poster cũ
- Hiển thị error toast nhưng không block update

### File validation:
```tsx
// Validate size
if (file.size > maxBytes) {
  toast.error(`Kích thước file không được vượt quá ${maxSizeMB}MB`);
  return;
}

// Validate type
if (!allowedTypes.includes(file.type)) {
  toast.error("Chỉ chấp nhận các định dạng: JPG, PNG, GIF, WEBP");
  return;
}
```

## Schema Changes

### Added posterUrl field:
```tsx
const formSchema = z.object({
  title: z.string().min(1, "Tên phim không được để trống"),
  durationMinutes: z.coerce.number().min(1, "Thời lượng phải lớn hơn 0"),
  description: z.string().optional(),
  trailerUrl: z.string().url().optional().or(z.literal("")),
  posterUrl: z.string().url().optional().or(z.literal("")), // ← NEW
  releaseDate: z.date(),
  status: z.enum(["COMING_SOON", "NOW_SHOWING", "ENDED", "CANCELLED"]),
  category: z.enum([...]),
});
```

## Testing Checklist

- [x] Load movie với poster → Hiển thị đúng poster
- [x] Load movie không có poster → Hiển thị placeholder
- [x] Select status → Hiển thị đúng giá trị hiện tại
- [x] Select category → Hiển thị đúng giá trị hiện tại
- [x] Thay đổi status → Submit OK
- [x] Thay đổi category → Submit OK
- [x] Upload poster mới → Submit OK, poster được update
- [x] Xóa poster → Submit OK, posterUrl = ""
- [x] File size > 10MB → Error toast, không upload
- [x] File type không phải ảnh → Error toast, không upload
- [x] Upload poster thất bại → Warning toast, giữ poster cũ, vẫn update movie

## Data Flow

### Load Movie:
```
Backend → API Response → mapBackendToForm → Form State → UI Display
  ↓          ↓              ↓                  ↓            ↓
"Showing" → {status:    → "NOW_SHOWING"    → form.status → Select shows
            "Showing"}                                      "Đang chiếu"

"Action"  → {category:  → "ACTION"         → form.category → Select shows
            "Action"}                                         "Hành động"
```

### Update Movie:
```
Form State → mapFormToBackend → API Request → Backend
    ↓            ↓                  ↓            ↓
"NOW_SHOWING" → 1 (MovieStatus.Showing) → Update → Database

"ACTION"      → 4 (MovieCategory.Action) → Update → Database
```

## Key Functions

### Status Mapping:
```tsx
// Backend → Form
const mapBackendStatusToForm = (status: string): FormValues['status']

// Form → Backend
const mapStatusToEnum = (status: string): number
```

### Category Mapping:
```tsx
// Backend → Form
const mapBackendCategoryToForm = (category: string): FormValues['category']

// Form → Backend
const mapCategoryToEnum = (category: string): number
```

## Notes

1. **Bi-directional Mapping**: Cần map 2 chiều vì backend dùng PascalCase và numeric enums, frontend dùng UPPERCASE strings

2. **Poster Upload**: Upload như CreateMovie - lưu file local, upload khi submit

3. **Error Recovery**: Nếu upload poster fail, vẫn update movie thành công với poster cũ

4. **Validation**: Client-side validation cho file size và type trước khi upload

5. **UI Feedback**: Toast messages cho từng bước (Loading, Uploading, Success/Error)
