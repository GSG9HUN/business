package com.dc.melodiasmario.core.common

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

class TrackSearchPrefixTest {
    @Test
    fun recognizesAllBotPrefixesAndAliases() {
        val expected = mapOf(
            TrackSearchPrefix.SPOTIFY to listOf("sptfy", "spotify"),
            TrackSearchPrefix.SOUNDCLOUD to listOf("scsearch", "soundcloud"),
            TrackSearchPrefix.YOUTUBE_MUSIC to listOf("ytmsearch", "youtubemusic"),
            TrackSearchPrefix.YOUTUBE to listOf("ytsearch", "youtube"),
            TrackSearchPrefix.APPLE_MUSIC to listOf("amsearch", "applemusic"),
            TrackSearchPrefix.DEEZER to listOf("dzsearch", "deezer"),
            TrackSearchPrefix.YANDEX_MUSIC to listOf("ymsearch", "yandexmusic"),
            TrackSearchPrefix.BANDCAMP to listOf("bcsearch", "bandcamp"),
        )
        assertEquals(expected.keys, TrackSearchPrefix.entries.toSet())
        expected.forEach { (provider, prefixes) ->
            assertEquals(prefixes, listOf(provider.prefix, provider.alias))
            prefixes.forEach { prefix ->
                assertEquals(provider, TrackSearchPrefix.fromInput("$prefix:track"))
                assertEquals(provider, TrackSearchPrefix.fromInput("${prefix.uppercase()}:track"))
                assertEquals(provider, TrackSearchPrefix.fromInput("$prefix:"))
            }
        }
    }

    @Test
    fun doesNotTreatUrlsOrUnprefixedQueriesAsSearchPrefixes() {
        listOf("", "youtube", "a song", "unknown:track", ":track",
            "https://youtube.com/watch?v=123", " youtube:track").forEach {
            assertNull(TrackSearchPrefix.fromInput(it))
        }
    }
}
