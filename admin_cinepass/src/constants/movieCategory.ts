export const MOVIE_CATEGORIES = [
  { value: "MOVIE", label: "Điện ảnh" },
  { value: "SERIES", label: "Phim bộ" },
  { value: "DOCUMENTARY", label: "Tài liệu" },
  { value: "ANIMATION", label: "Hoạt hình" },
  { value: "ACTION", label: "Hành động" },
  { value: "COMEDY", label: "Hài" },
  { value: "DRAMA", label: "Chính kịch" },
  { value: "HORROR", label: "Kinh dị" },
  { value: "ROMANCE", label: "Lãng mạn" },
  { value: "SCIFI", label: "Khoa học viễn tưởng" },
  { value: "THRILLER", label: "Giật gân" },
  { value: "WAR", label: "Chiến tranh" },
  { value: "WESTERN", label: "Viễn Tây" },
  { value: "MUSICAL", label: "Nhạc kịch" },
  { value: "FAMILY", label: "Gia đình" },
  { value: "FANTASY", label: "Giả tưởng" },
  { value: "ADVENTURE", label: "Phiêu lưu" },
  { value: "BIOGRAPHY", label: "Tiểu sử" },
  { value: "HISTORY", label: "Lịch sử" },
  { value: "SPORT", label: "Thể thao" },
  { value: "RELIGIOUS", label: "Tôn giáo" },
  { value: "OTHER", label: "Khác" },
] as const;

export const CATEGORY_LABEL_MAP = Object.fromEntries(
  MOVIE_CATEGORIES.map(c => [c.value, c.label])
);
