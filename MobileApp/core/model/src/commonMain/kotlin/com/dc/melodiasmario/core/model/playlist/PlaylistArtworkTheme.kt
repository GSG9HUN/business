package com.dc.melodiasmario.core.model.playlist

/** Local fallback themes inferred from playlist-name words, without inferring track genres. */
enum class PlaylistArtworkTheme {
    Default,
    NightDrive,
    Chill,
    Workout;

    companion object {
        fun fromName(name: String): PlaylistArtworkTheme {
            val words = Regex("\\p{L}+").findAll(name.lowercase()).map { it.value }.toSet()
            return when {
                words.any {
                    it in setOf(
                        "workout",
                        "gym",
                        "fitness",
                        "training",
                        "edzés",
                        "edzes"
                    )
                } -> Workout

                words.any {
                    it in setOf(
                        "chill",
                        "relax",
                        "lofi",
                        "nyugi",
                        "pihenés",
                        "pihenes"
                    )
                } -> Chill

                words.any {
                    it in setOf(
                        "night",
                        "drive",
                        "synthwave",
                        "éjszaka",
                        "éjszakai"
                    )
                } -> NightDrive

                else -> Default
            }
        }
    }
}
