package com.dc.melodiasmario.shared.di

import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module

@Module(includes = [NetworkModule::class])
@ComponentScan("com.dc.melodiasmario.shared")
class SharedModule
