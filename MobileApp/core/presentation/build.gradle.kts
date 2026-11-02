plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.android.lint)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.presentation"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24

        withHostTestBuilder { }
        withDeviceTestBuilder {
            sourceSetTreeName = "test"
        }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "presentationKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            dependencies {
                api(dependencyNotation = project(":core:common"))
                api(dependencyNotation = project(":core:model"))
                api(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.kotlin.stdlib)
            }
        }
        commonTest {
            dependencies {
                implementation(dependencyNotation = libs.kotlin.test)
                implementation(dependencyNotation = libs.kotlinx.coroutines.test)
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
