package com.dc.melodiasmario.feature.audioeffect.di

import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module

/** Registers audio-effect presentation dependencies independently of CurrentTrack. */
@Module
@ComponentScan("com.dc.melodiasmario.feature.audioeffect.presentation")
class AudioEffectModule
