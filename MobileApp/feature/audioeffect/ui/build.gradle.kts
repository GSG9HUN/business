plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.android.lint)
}

compose.resources {
    packageOfResClass = "com.dc.melodiasmario.feature.audioeffect.generated.resources"
    publicResClass = true
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.feature.audioeffect.ui"
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
    }

    val xcfName = "audioeffectKitUi"

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
                implementation(dependencyNotation = project(":feature:audioeffect:presentation"))
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:common-ui"))
                implementation(dependencyNotation = project(":core:model"))
                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.androidx.lifecycle.viewmodelCompose)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.compose.runtime)
                implementation(dependencyNotation = libs.compose.foundation)
                implementation(dependencyNotation = libs.compose.material3)
                implementation(dependencyNotation = libs.compose.ui)
                implementation(dependencyNotation = libs.compose.components.resources)
                implementation(dependencyNotation = libs.compose.uiToolingPreview)
                implementation(dependencyNotation = libs.jetbrains.navigation3.ui)
                implementation(dependencyNotation = libs.koin.compose)
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
