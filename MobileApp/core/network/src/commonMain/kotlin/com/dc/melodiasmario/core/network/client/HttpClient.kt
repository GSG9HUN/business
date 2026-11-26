package com.dc.melodiasmario.core.network.client

import io.ktor.client.HttpClient
import io.ktor.client.HttpClientConfig
import io.ktor.client.plugins.contentnegotiation.ContentNegotiation
import io.ktor.client.plugins.logging.DEFAULT
import io.ktor.client.plugins.logging.LogLevel
import io.ktor.client.plugins.logging.Logger
import io.ktor.client.plugins.logging.Logging
import io.ktor.http.HttpHeaders
import io.ktor.serialization.kotlinx.json.json
import kotlinx.serialization.json.Json

/**
 * Creates the platform HTTP client with the shared serialization and logging configuration.
 */
fun createHttpClient(): HttpClient {
    return HttpClient {
        configureMelodiasHttpClient()
    }
}

/**
 * Installs HTTP status validation, JSON serialization, and logging with sanitized authorization headers.
 * Requests with non-success status codes throw; status probes may explicitly disable validation.
 */
internal fun HttpClientConfig<*>.configureMelodiasHttpClient(
    logger: Logger = Logger.DEFAULT
) {
    expectSuccess = true

    install(plugin = ContentNegotiation) {
        json(
            json = Json {
                ignoreUnknownKeys = true
                isLenient = true
            }
        )
    }
    install(plugin = Logging) {
        this.logger = logger
        level = LogLevel.HEADERS

        sanitizeHeader { header ->
            header == HttpHeaders.Authorization
        }
    }
}
