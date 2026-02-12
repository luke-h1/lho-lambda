import Foundation

actor MemoryCache {
    private var cache: [String: CacheEntry] = [:]

    struct CacheEntry {
        let value: Any
        let expirationDate: Date
    }

    func get<T>(_ key: String) -> T? {
        guard let entry = cache[key] else {
            return nil
        }

        if entry.expirationDate < Date() {
            cache.removeValue(forKey: key)
            return nil
        }

        return entry.value as? T
    }

    func set<T>(_ key: String, value: T, expiration: TimeInterval) {
        let expirationDate = Date().addingTimeInterval(expiration)
        cache[key] = CacheEntry(value: value, expirationDate: expirationDate)
    }

    func remove(_ key: String) {
        cache.removeValue(forKey: key)
    }

    func clear() {
        cache.removeAll()
    }
}
