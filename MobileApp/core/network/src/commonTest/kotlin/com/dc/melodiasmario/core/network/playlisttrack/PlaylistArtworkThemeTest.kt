package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.model.playlist.PlaylistArtworkTheme
import kotlin.test.Test
import kotlin.test.assertEquals

class PlaylistArtworkThemeTest {
    @Test
    fun themeMatchingUsesWholeWordsAndIgnoresCase(): Unit {
        assertEquals(expected = PlaylistArtworkTheme.NightDrive, actual = PlaylistArtworkTheme.fromName(name = "NIGHT DRIVE 2026"))
        assertEquals(expected = PlaylistArtworkTheme.Chill, actual = PlaylistArtworkTheme.fromName(name = "Sunday: Chill & Relax"))
        assertEquals(expected = PlaylistArtworkTheme.Workout, actual = PlaylistArtworkTheme.fromName(name = "Reggeli EDZÉS"))
        assertEquals(expected = PlaylistArtworkTheme.Default, actual = PlaylistArtworkTheme.fromName(name = "Chilli peppers"))
        assertEquals(expected = PlaylistArtworkTheme.Default, actual = PlaylistArtworkTheme.fromName(name = ""))
    }

    @Test
    fun ambiguousNamesHaveStableThemePriority(): Unit {
        assertEquals(expected = PlaylistArtworkTheme.Workout, actual = PlaylistArtworkTheme.fromName(name = "Night workout"))
        assertEquals(expected = PlaylistArtworkTheme.Chill, actual = PlaylistArtworkTheme.fromName(name = "Chill night drive"))
    }
}
