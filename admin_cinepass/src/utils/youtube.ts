/**
 * Chuyển YouTube URL thông thường sang embed URL
 * Hỗ trợ:
 * - https://www.youtube.com/watch?v=VIDEO_ID
 * - https://youtu.be/VIDEO_ID
 * - https://www.youtube.com/embed/VIDEO_ID
 */
export const getYoutubeEmbedUrl = (url?: string | null): string | null => {
    if (!url) return null;
  
    try {
      // watch?v=
      const watchMatch = url.match(/[?&]v=([^&]+)/);
      if (watchMatch?.[1]) {
        return `https://www.youtube.com/embed/${watchMatch[1]}`;
      }
  
      // youtu.be/
      const shortMatch = url.match(/youtu\.be\/([^?]+)/);
      if (shortMatch?.[1]) {
        return `https://www.youtube.com/embed/${shortMatch[1]}`;
      }
  
      // embed
      if (url.includes("/embed/")) {
        return url;
      }
  
      return null;
    } catch {
      return null;
    }
  };
  