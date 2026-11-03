package com.dc.melodiasmario.core.common

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

class TrackSearchPrefixTest {
    @Test
    fun recognizesAllBotPrefixesAndAliases(): Unit {
        val expected = mapOf(
            TrackSearchPrefix.SPOTIFY to listOf("sptfy", "spotify"),
            TrackSearchPrefix.SOUNDCLOUD to listOf("scsearch", "soundcloud"),
            TrackSearchPrefix.YOUTUBE_MUSIC to listOf("ytmsearch", "youtubemusic"),
            TrackSearchPrefix.YOUTUBE to listOf("ytsearch", "youtube"),
            TrackSearchPrefix.APPLE_MUSIC to listOf("amsearch", "applemusic"),
            TrackSearchPrefix.DEEZER to listOf("dzsearch", "deezer"),
            TrackSearchPrefix.YANDEX_MUSIC to listOf("ymsearch", "yandexmusic"),
            TrackSearchPrefix.BANDCAMP to listOf("bcsearch", "bandcamp")
        )
        assertEquals(expected = expected.keys, actual = TrackSearchPrefix.entries.toSet())
        expected.forEach { (provider, prefixes) ->
            assertEquals(expected = prefixes, actual = listOf(provider.prefix, provider.alias))
            prefixes.forEach { prefix ->
                assertEquals(expected = provider, actual = TrackSearchPrefix.fromInput(input = "$prefix:track"))
                assertEquals(expected = provider, actual = TrackSearchPrefix.fromInput(input = "${prefix.uppercase()}:track"))
                assertEquals(expected = provider, actual = TrackSearchPrefix.fromInput(input = "$prefix:"))
            }
        }
    }

    @Test
    fun doesNotTreatUrlsOrUnprefixedQueriesAsSearchPrefixes(): Unit {
        listOf(
            "",
            "youtube",
            "a song",
            "unknown:track",
            ":track",
            "https://youtube.com/watch?v=123",
            " youtube:track"
        ).forEach { inputQuery ->
            assertNull(actual = TrackSearchPrefix.fromInput(input = inputQuery))
        }
    }
}
