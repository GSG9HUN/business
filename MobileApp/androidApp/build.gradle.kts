import org.jetbrains.kotlin.gradle.dsl.JvmTarget

plugins {
    alias(libs.plugins.androidApplication)
    alias(libs.plugins.composeMultiplatform)
    alias(libs.plugins.composeCompiler)
}

kotlin {
    compilerOptions {
        jvmTarget = JvmTarget.JVM_11
    }
}
dependencies {
    implementation(project(":shared"))
    implementation(project(":core:common"))
    implementation(project(":core:common-ui"))
    implementation(project(":core:di"))
    implementation(project(":feature:playlistsong:presentation"))
    implementation(project(":feature:playlistsong:ui"))
    implementation(project(":feature:currenttrack:presentation"))
    implementation(project(":feature:currenttrack:ui"))
    implementation(project(":feature:guild:presentation"))
    implementation(project(":feature:guild:ui"))
    implementation(project(":feature:login:presentation"))
    implementation(project(":feature:login:ui"))
    implementation(project(":feature:playlist:presentation"))
    implementation(project(":feature:playlist:ui"))
    implementation(project(":feature:profile:presentation"))
    implementation(project(":feature:profile:ui"))
    implementation(project(":feature:queue:presentation"))
    implementation(project(":feature:queue:ui"))
    implementation(project(":feature:removesong:presentation"))
    implementation(project(":feature:removesong:ui"))
    implementation(project(":feature:settings:presentation"))
    implementation(project(":feature:settings:ui"))

    implementation(libs.androidx.activity.compose)
    implementation(libs.androidx.lifecycle.runtimeCompose)

    implementation(libs.compose.uiToolingPreview)
    debugImplementation(libs.compose.uiTooling)

    implementation(libs.koin.android)
}

android {
    namespace = "com.dc.melodiasmario"
    compileSdk = libs.versions.android.compileSdk.get().toInt()

    defaultConfig {
        applicationId = "com.dc.melodiasmario"
        minSdk = libs.versions.android.minSdk.get().toInt()
        targetSdk = libs.versions.android.targetSdk.get().toInt()
        versionCode = 1
        versionName = "1.0"
    }
    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }
    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    buildFeatures {
        compose = true
    }
}


