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
                api(project(":core:common"))
                api(project(":core:model"))
                api(project(":core:network"))
                api(project(":core:datastore"))
                api(project(":core:domain"))
                api(project(":core:data"))

                implementation(libs.kotlin.stdlib)
                implementation(libs.koin.core)
                implementation(libs.koin.annotations)
                implementation(libs.ktor.client.core)
            }
        }

        commonTest { dependencies { implementation(libs.kotlin.test) } }
        getByName("androidDeviceTest") {
            dependencies {
                implementation(libs.androidx.core)
                implementation(libs.androidx.junit)
                implementation(libs.androidx.runner)
            }
        }
    }
}
