package com.dc.melodiasmario.feature.audioeffect.presentation

/** One-time feedback; command acceptance alone never produces an applied notification. */
enum class AudioEffectsEffect { Applied, PresetSaved, PresetDeleted, Failed, TimedOut, NavigateBack }
