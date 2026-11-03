package com.dc.melodiasmario.core.domain.di

import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module

/**
 * Registers domain dependencies with Koin.
 */
@Module
@ComponentScan("com.dc.melodiasmario.core.domain")
class DomainModule
