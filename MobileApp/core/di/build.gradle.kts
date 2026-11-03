plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.android.lint)
    alias(libs.plugins.ksp)
    alias(libs.plugins.koin.compiler)
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.di"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24

        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "coreDiKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            kotlin.srcDir("build/generated/ksp/metadata/commonMain/kotlin")
            dependencies {
                api(dependencyNotation = project(":core:common"))
                api(dependencyNotation = project(":core:model"))
                api(dependencyNotation = project(":core:network"))
                api(dependencyNotation = project(":core:datastore"))
                api(dependencyNotation = project(":core:domain"))
                api(dependencyNotation = project(":core:data"))

                implementation(dependencyNotation = libs.kotlin.stdlib)
                implementation(dependencyNotation = libs.koin.core)
                implementation(dependencyNotation = libs.koin.annotations)
                implementation(dependencyNotation = libs.ktor.client.core)
            }
        }

        commonTest { dependencies { implementation(dependencyNotation = libs.kotlin.test) } }
        getByName("androidDeviceTest") {
            dependencies {
                implementation(dependencyNotation = libs.androidx.core)
                implementation(dependencyNotation = libs.androidx.junit)
                implementation(dependencyNotation = libs.androidx.runner)
            }
        }
    }
}
