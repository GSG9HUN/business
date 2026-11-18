plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.jetbrains.kotlin.serialization)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
    alias(libs.plugins.android.lint)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.feature.settings.presentation"
        compileSdk {
            version = release(36) {
                minorApiLevel = 1
            }
        }
        minSdk = 24

        withHostTestBuilder {
        }

        withDeviceTestBuilder {
            sourceSetTreeName = "test"
        }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "settingsKitPresentation"

    iosArm64 {
        binaries.framework {
            baseName = xcfName
        }
    }

    iosSimulatorArm64 {
        binaries.framework {
            baseName = xcfName
        }
    }

    sourceSets {
        commonMain {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:domain"))
                implementation(dependencyNotation = project(":core:model"))

                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.androidx.lifecycle.viewmodelCompose)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.annotations)
                implementation(dependencyNotation = libs.koin.compose.viewmodel)
                implementation(dependencyNotation = libs.kotlinx.serialization.json)
                implementation(dependencyNotation = libs.ktor.client.core)
            }
        }

        commonTest {
            dependencies {
                implementation(dependencyNotation = libs.kotlin.test)
            }
        }

        getByName("androidDeviceTest") {
            dependencies {
                implementation(dependencyNotation = libs.androidx.core)
                implementation(dependencyNotation = libs.androidx.junit)
                implementation(dependencyNotation = libs.androidx.runner)
            }
        }
    }
}
