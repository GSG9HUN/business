package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.model.playlist.PlaylistArtworkTheme
import kotlin.test.Test
import kotlin.test.assertEquals

class PlaylistArtworkThemeTest {
    @Test
    fun themeMatchingUsesWholeWordsAndIgnoresCase() {
        assertEquals(PlaylistArtworkTheme.NightDrive, PlaylistArtworkTheme.fromName("NIGHT DRIVE 2026"))
        assertEquals(PlaylistArtworkTheme.Chill, PlaylistArtworkTheme.fromName("Sunday: Chill & Relax"))
        assertEquals(PlaylistArtworkTheme.Workout, PlaylistArtworkTheme.fromName("Reggeli EDZÉS"))
        assertEquals(PlaylistArtworkTheme.Default, PlaylistArtworkTheme.fromName("Chilli peppers"))
        assertEquals(PlaylistArtworkTheme.Default, PlaylistArtworkTheme.fromName(""))
    }

    @Test
    fun ambiguousNamesHaveStableThemePriority() {
        assertEquals(PlaylistArtworkTheme.Workout, PlaylistArtworkTheme.fromName("Night workout"))
        assertEquals(PlaylistArtworkTheme.Chill, PlaylistArtworkTheme.fromName("Chill night drive"))
    }
}
