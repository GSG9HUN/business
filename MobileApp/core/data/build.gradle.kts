plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.android.lint)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.data"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24
        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }
    val xcfName = "dataKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }
    sourceSets {
        commonMain {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
            dependencies {
                implementation(dependencyNotation = project(":core:common"))
                implementation(dependencyNotation = project(":core:model"))
                implementation(dependencyNotation = project(":core:domain"))
                implementation(dependencyNotation = project(":core:network"))
                implementation(dependencyNotation = project(":core:datastore"))
                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.kotlinx.coroutines.core)
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
    }
}
