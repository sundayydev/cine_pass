# MovieDetail.tsx - Cập nhật UI cho Actors và Reviews

## Tổng quan
Đã cập nhật trang chi tiết phim (`MovieDetail.tsx`) để hiển thị thông tin diễn viên và đánh giá của khán giả với UI đẹp mắt sử dụng shadcn và tailwindcss.

## Các thay đổi chính

### 1. **Import mới**
- `MovieDetailResponseDto`, `MovieActorDto`, `MovieReviewDto` từ `@/services/apiMovie`
- `Avatar`, `AvatarFallback`, `AvatarImage` từ shadcn
- Icons mới: `User2`, `Star`, `MessageSquare` từ lucide-react

### 2. **Cập nhật Type**
- Thay đổi từ `Movie` sang `MovieDetailResponseDto` để nhận được thông tin actors và reviews từ API

### 3. **Rating Summary Card** (Bên trái)
- Card hiển thị điểm đánh giá trung bình
- Sử dụng gradient từ yellow đến orange
- Hiển thị số sao và tổng số đánh giá
- Chỉ hiển thị khi có đánh giá

**Tính năng:**
- ⭐ Điểm đánh giá trung bình (số lớn)
- 🌟 5 sao màu vàng
- 📊 Tổng số đánh giá

### 4. **Actors Section** (Cột phải)
Card hiển thị danh sách diễn viên với:

**Thiết kế:**
- Header gradient từ purple đến pink
- Grid responsive: 2 → 3 → 4 cột
- Mỗi actor card có:
  - Ảnh diễn viên với aspect ratio 3:4
  - Hover effect: scale + shadow
  - Gradient overlay khi hover
  - Thông tin mô tả hiện khi hover
  - Fallback gradient nếu không có ảnh

**Tính năng:**
- 👤 Avatar/Photo của diễn viên
- 📝 Tên diễn viên
- 📄 Mô tả vai trò (hiện khi hover)
- 🎨 Smooth animations và transitions

### 5. **Reviews Section** (Cột phải)
Card hiển thị đánh giá từ khán giả với:

**Thiết kế:**
- Header gradient từ blue đến cyan
- Mỗi review card có:
  - Avatar với gradient background
  - Username và ngày đánh giá
  - 5 sao rating
  - Comment/nội dung đánh giá
  - Hover effect: background accent + shadow

**Tính năng:**
- 👤 Avatar người đánh giá (chữ cái đầu)
- ⭐ Rating với 5 sao màu vàng
- 💬 Comment của người dùng
- 📅 Thời gian đánh giá
- 🎨 Hover effects và animations

### 6. **Poster Enhancement**
Cải thiện hiển thị poster:
- Hover effect với scale transform
- Gradient overlay khi hover
- Smooth transition

## Các Design Elements Được Sử dụng

### ✨ **Modern Aesthetics**
1. **Gradients**
   - Yellow-Orange cho rating
   - Purple-Pink cho actors
   - Blue-Cyan cho reviews

2. **Hover Effects**
   - Scale transforms (1.05, 1.1)
   - Shadow elevation
   - Opacity transitions
   - Color transitions

3. **Typography**
   - Font weights đa dạng
   - Line clamping
   - Proper hierarchy

4. **Colors**
   - Vibrant gradient backgrounds
   - Muted foreground text
   - Primary color accents

### 🎨 **Components Used**
- Card với gradient headers
- Avatar với fallback
- Badge cho count
- Separator
- Icons từ lucide-react

## API Integration

Sử dụng `MovieDetailResponseDto` từ backend, bao gồm:
```typescript
interface MovieDetailResponseDto {
  // ... thông tin phim cơ bản
  actors: MovieActorDto[];      // Danh sách diễn viên
  reviews: MovieReviewDto[];    // Danh sách đánh giá
}
```

## Responsive Design

- **Mobile**: 2 cột cho actors
- **Tablet (sm)**: 3 cột cho actors  
- **Desktop (md)**: 4 cột cho actors
- Grid layout tự động điều chỉnh

## Accessibility

- Semantic HTML
- Alt text cho images
- ARIA labels
- Keyboard navigation support
- Color contrast

## Performance Optimizations

- Lazy loading cho images
- CSS transitions thay vì animations
- Conditional rendering (chỉ render khi có data)
- Error fallback cho images

## Kết quả

Trang chi tiết phim giờ đây có:
- ✅ Hiển thị đầy đủ thông tin diễn viên với ảnh
- ✅ Hiển thị đánh giá từ khán giả
- ✅ UI đẹp mắt, premium
- ✅ Animations và transitions mượt mà
- ✅ Responsive trên mọi thiết bị
- ✅ Dark mode support
