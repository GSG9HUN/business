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

fun createHttpClient(): HttpClient {
    return HttpClient {
        configureMelodiasHttpClient()
    }
}

internal fun HttpClientConfig<*>.configureMelodiasHttpClient(
    logger: Logger = Logger.DEFAULT
) {
    install(ContentNegotiation) {
        json(
            Json {
                ignoreUnknownKeys = true
                isLenient = true
            }
        )
    }
    install(Logging) {
        this.logger = logger
        level = LogLevel.HEADERS

        sanitizeHeader { header ->
            header == HttpHeaders.Authorization
        }
    }
}
