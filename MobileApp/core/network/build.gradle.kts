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
        namespace = "com.dc.melodiasmario.core.network"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24
        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "networkKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:model"))
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.annotations)
                implementation(dependencyNotation = libs.ktor.client.core)
                implementation(dependencyNotation = libs.ktor.client.logging)
                implementation(dependencyNotation = libs.ktor.client.content.negotiation)
                implementation(dependencyNotation = libs.ktor.serialization.kotlinx.json)
                implementation(dependencyNotation = libs.kotlinx.serialization.json)
            }
        }
        commonTest {
            dependencies {
                implementation(dependencyNotation = libs.kotlin.test)
                implementation(dependencyNotation = libs.kotlinx.coroutines.test)
                implementation(dependencyNotation = libs.ktor.client.mock)
            }
        }
        androidMain {
            dependencies {
                implementation(dependencyNotation = libs.ktor.client.okhttp)
                implementation(dependencyNotation = libs.microsoft.signalr)
                implementation(dependencyNotation = libs.rxjava3)
            }
        }
        iosMain {
            dependencies {
                implementation(dependencyNotation = libs.ktor.client.darwin)
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
