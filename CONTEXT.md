# Context

## Domain Terms

### Now Playing

The public music status exposed by this Lambda. The live route shape is `/api/now-playing`.

### Last.fm Now-Playing Source

Last.fm is the source of truth for Now Playing. Spotify may still be used when explicitly requested, but callers should not need to treat Last.fm and Spotify as equal sources of truth.

### Spotify Now-Playing Path

`/api/now-playing?provider=spotify` may remain as an explicit path for checking Spotify behaviour. It is not the main Now Playing source and should not shape the main Now Playing interface.

`/api/now-playing?provider=lastfm` is a compatibility alias for the main Now Playing path. It should behave the same as omitting the provider query parameter.

### Empty Now-Playing View Model

When the Last.fm lookup fails, `/api/now-playing` should still return an empty Now Playing view model with `status: 500` in the response body. This preserves the client-facing shape while making the failure visible to callers that inspect the model.

### Now-Playing Cache Policy

Both Last.fm Now Playing and the explicit Spotify path should be cached. Cache behaviour belongs inside the Now Playing module so callers do not need to know provider-specific cache keys or freshness rules.

### Runtime Configuration

Runtime Configuration is the environment-backed settings required by the Lambda functions at execution time. It includes music provider credentials, deployment metadata, observability settings, and feature flags.

Runtime Configuration should be one shared module used by the API Lambda, authorizer, and observability code. Observability should not maintain a separate environment reader.

Runtime Configuration should validate lazily by section. A missing provider secret should fail only the path that needs that provider. Missing Sentry or Pushgateway settings should disable that sink rather than fail requests. Missing authorizer API key should continue to deny authorization instead of throwing.

Runtime Configuration should expose typed section objects instead of raw environment-variable lookups. Sections should own their defaults, optional sink behaviour, and provider-specific credential rules.

Runtime Configuration should use typed `*Options` classes with `Section` constants, init properties/defaults, and targeted `IsValid(out errorMessage)` validation where a section has required values.
