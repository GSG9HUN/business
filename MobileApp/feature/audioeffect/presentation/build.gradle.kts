plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
    alias(libs.plugins.android.lint)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.feature.audioeffect.presentation"
        compileSdk {
            version = release(36) {
                minorApiLevel = 1
            }
        }
        minSdk = 24

        withHostTestBuilder {
        }
    }

    val xcfName = "audioeffectKitPresentation"

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
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:presentation"))
                implementation(dependencyNotation = project(":core:domain"))
                implementation(dependencyNotation = project(":core:model"))
                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.androidx.lifecycle.viewmodelCompose)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.annotations)
                implementation(dependencyNotation = libs.koin.compose.viewmodel)
            }
        }

        commonTest {
            dependencies {
                implementation(dependencyNotation = libs.kotlin.test)
            }
        }
    }
}
