import time
import threading
from typing import Any, Optional, Dict, Tuple


class InMemoryTTLCache:
    """
    Bộ nhớ đệm trong RAM (In-Memory) an toàn luồng (Thread-safe)
    hỗ trợ thời gian sống (TTL - Time To Live) cho từng mục.
    """

    def __init__(self, default_ttl: int = 300, max_size: int = 2000):
        self._default_ttl = default_ttl
        self._max_size = max_size
        self._cache: Dict[str, Tuple[Any, float]] = {}  # key -> (value, expire_at)
        self._lock = threading.RLock()
        self._hits = 0
        self._misses = 0

    def get(self, key: str) -> Optional[Any]:
        """
        Lấy giá trị từ cache. Trả về None nếu không tồn tại hoặc đã hết hạn.
        """
        with self._lock:
            if key not in self._cache:
                self._misses += 1
                return None

            value, expire_at = self._cache[key]
            if time.time() > expire_at:
                del self._cache[key]
                self._misses += 1
                return None

            self._hits += 1
            return value

    def set(self, key: str, value: Any, ttl: Optional[int] = None) -> None:
        """
        Lưu giá trị vào cache với thời gian sống (TTL) tính bằng giây.
        """
        duration = ttl if ttl is not None else self._default_ttl
        expire_at = time.time() + duration

        with self._lock:
            # Thu dọn các key hết hạn nếu danh sách vượt quá max_size
            if len(self._cache) >= self._max_size:
                self._purge_expired()

            self._cache[key] = (value, expire_at)

    def delete(self, key: str) -> bool:
        """
        Xóa một mục cụ thể trong cache.
        """
        with self._lock:
            if key in self._cache:
                del self._cache[key]
                return True
            return False

    def clear(self) -> None:
        """
        Xóa sạch toàn bộ bộ nhớ đệm.
        """
        with self._lock:
            self._cache.clear()

    def _purge_expired(self) -> None:
        """
        Dọn dẹp các bản ghi đã hết hạn để giải phóng bộ nhớ.
        """
        now = time.time()
        expired_keys = [k for k, (_, exp) in self._cache.items() if now > exp]
        for k in expired_keys:
            del self._cache[k]

    def stats(self) -> Dict[str, Any]:
        """
        Thống kê hiệu quả hoạt động của bộ nhớ đệm.
        """
        with self._lock:
            now = time.time()
            active_items = sum(1 for _, exp in self._cache.values() if exp > now)
            total_requests = self._hits + self._misses
            hit_ratio = round(self._hits / total_requests, 4) if total_requests > 0 else 0.0
            return {
                "active_items": active_items,
                "hits": self._hits,
                "misses": self._misses,
                "hit_ratio": hit_ratio,
            }


# Khởi tạo một phiên bản cache dùng chung toàn hệ thống
cache = InMemoryTTLCache()
