plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.jetbrains.kotlin.serialization)
    alias(libs.plugins.android.lint)
}

compose.resources {
    packageOfResClass = "com.dc.melodiasmario.feature.playlisttrack.generated.resources"
    publicResClass = true
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.feature.playlisttrack.ui"
        compileSdk {
            version = release(36) {
                minorApiLevel = 1
            }
        }
        minSdk = 24

        androidResources {
            enable = true
        }

        withHostTestBuilder {
        }

        withDeviceTestBuilder {
            sourceSetTreeName = "test"
        }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "playlisttrackKitUi"

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
                implementation(dependencyNotation = project(":feature:playlisttrack:presentation"))
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:common-ui"))
                implementation(dependencyNotation = project(":core:domain"))
                implementation(dependencyNotation = project(":core:model"))

                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.compose.runtime)
                implementation(dependencyNotation = libs.compose.foundation)
                implementation(dependencyNotation = libs.compose.material3)
                implementation(dependencyNotation = libs.compose.ui)
                implementation(dependencyNotation = libs.compose.components.resources)
                implementation(dependencyNotation = libs.compose.uiToolingPreview)
                implementation(dependencyNotation = libs.androidx.lifecycle.viewmodelCompose)
                implementation(dependencyNotation = libs.androidx.lifecycle.runtimeCompose)
                implementation(dependencyNotation = libs.jetbrains.navigation3.ui)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.compose)
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

        androidMain {
            dependencies {
                implementation(dependencyNotation = libs.compose.uiTooling)
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
