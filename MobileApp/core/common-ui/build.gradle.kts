plugins {
    alias(libs.plugins.kotlinMultiplatform)
    alias(libs.plugins.androidMultiplatformLibrary)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
    alias(libs.plugins.android.lint)
}

compose.resources {
    packageOfResClass = "com.dc.melodiasmario.core.commonui.designsystem.generated.resources"
    publicResClass = true
}

kotlin {
    android {
        namespace = "com.dc.melodiasmario.core.commonui"
        compileSdk { version = release(36) { minorApiLevel = 1 } }
        minSdk = 24

        androidResources { enable = true }
        withHostTestBuilder { }
        withDeviceTestBuilder { sourceSetTreeName = "test" }.configure {
            instrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        }
    }

    val xcfName = "uiKit"
    iosArm64 { binaries.framework { baseName = xcfName } }
    iosSimulatorArm64 { binaries.framework { baseName = xcfName } }

    sourceSets {
        commonMain {
            dependencies {
                implementation(project(":core:common"))

                implementation(libs.kotlin.stdlib)
                implementation(libs.compose.runtime)
                implementation(libs.compose.foundation)
                implementation(libs.compose.material3)
                implementation(libs.compose.ui)
                implementation(libs.compose.components.resources)
                implementation(libs.compose.uiToolingPreview)
                implementation(libs.coil.compose)
                implementation(libs.coil.network.ktor3)
            }
        }

        commonTest { dependencies { implementation(libs.kotlin.test) } }
        androidMain { dependencies { implementation(libs.compose.uiTooling) } }
        getByName("androidDeviceTest") {
            dependencies {
                implementation(libs.androidx.core)
                implementation(libs.androidx.junit)
                implementation(libs.androidx.runner)
            }
        }
    }
}
