package com.dc.melodiasmario.core.model.playlist

/** Local fallback themes inferred from playlist-name words, without inferring track genres. */
enum class PlaylistArtworkTheme {
    Default,
    NightDrive,
    Chill,
    Workout;

    /**
     * Shared values and operations for the enclosing type.
     */
    companion object {
        /**
         * Provides the from name operation for companion.
         */
        fun fromName(name: String): PlaylistArtworkTheme {
            val words = Regex("\\p{L}+").findAll(name.lowercase())
                .map { wordMatch -> wordMatch.value }
                .toSet()
            return when {
                words.any { word ->
                    word in setOf(
                        "workout",
                        "gym",
                        "fitness",
                        "training",
                        "edzés",
                        "edzes"
                    )
                } -> Workout

                words.any { word ->
                    word in setOf(
                        "chill",
                        "relax",
                        "lofi",
                        "nyugi",
                        "pihenés",
                        "pihenes"
                    )
                } -> Chill

                words.any { word ->
                    word in setOf(
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
