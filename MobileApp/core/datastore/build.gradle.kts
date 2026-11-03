plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
    alias(libs.plugins.android.lint)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.datastore"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24
        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "datastoreKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:domain"))
                implementation(dependencyNotation = project(":core:model"))
                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.annotations)
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

        getByName("androidHostTest") {
            dependencies {
                implementation(dependencyNotation = libs.androidx.core)
                implementation(dependencyNotation = libs.robolectric)
            }
        }

        getByName("androidMain") {
            dependencies {
                implementation(dependencyNotation = libs.androidx.datastore.preferences)
                implementation(dependencyNotation = libs.kotlinx.serialization.json)
            }
        }
    }
}
