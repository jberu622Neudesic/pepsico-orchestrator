#!/usr/bin/env node

/**
 * Sync Deep Links Script
 * 
 * This script syncs deep link schemes from constants/deepLinks.ts
 * to the native configuration files (Info.plist and AndroidManifest.xml)
 * 
 * Run with: node scripts/syncDeepLinks.js
 */

const fs = require('fs');
const path = require('path');

// Read the constants file
const constantsPath = path.join(__dirname, '../constants/deepLinks.ts');
const constantsContent = fs.readFileSync(constantsPath, 'utf8');

// Extract deep link schemes using regex
const mauiMatch = constantsContent.match(/MAUI_APP:\s*['"]([^'"]+)['"]/);
const flnaMatch = constantsContent.match(/FLNA:\s*['"]([^'"]+)['"]/);

if (!mauiMatch || !flnaMatch) {
  console.error('Error: Could not extract deep link schemes from constants file');
  process.exit(1);
}

const mauiScheme = mauiMatch[1];
const flnaScheme = flnaMatch[1];

console.log(`Found deep link schemes: ${mauiScheme}, ${flnaScheme}`);

// Update Info.plist
const infoPlistPath = path.join(__dirname, '../ios/FLNALauncher/Info.plist');
let infoPlistContent = fs.readFileSync(infoPlistPath, 'utf8');

// Replace the schemes in LSApplicationQueriesSchemes
infoPlistContent = infoPlistContent.replace(
  /<key>LSApplicationQueriesSchemes<\/key>\s*<array>\s*<string>[^<]+<\/string>\s*<string>[^<]+<\/string>\s*<\/array>/,
  `<key>LSApplicationQueriesSchemes</key>
	<array>
		<string>${mauiScheme}</string>
		<string>${flnaScheme}</string>
	</array>`
);

fs.writeFileSync(infoPlistPath, infoPlistContent);
console.log('✓ Updated ios/FLNALauncher/Info.plist');

// Update AndroidManifest.xml
const manifestPath = path.join(__dirname, '../android/app/src/main/AndroidManifest.xml');
let manifestContent = fs.readFileSync(manifestPath, 'utf8');

// Replace the schemes in queries section
manifestContent = manifestContent.replace(
  /<queries>\s*<intent>\s*<action android:name="android\.intent\.action\.VIEW" \/>\s*<data android:scheme="[^"]+" \/>\s*<\/intent>\s*<intent>\s*<action android:name="android\.intent\.action\.VIEW" \/>\s*<data android:scheme="[^"]+" \/>\s*<\/intent>\s*<\/queries>/,
  `    <queries>
        <intent>
            <action android:name="android.intent.action.VIEW" />
            <data android:scheme="${mauiScheme}" />
        </intent>
        <intent>
            <action android:name="android.intent.action.VIEW" />
            <data android:scheme="${flnaScheme}" />
        </intent>
    </queries>`
);

fs.writeFileSync(manifestPath, manifestContent);
console.log('✓ Updated android/app/src/main/AndroidManifest.xml');

console.log('\n✅ Deep links synced successfully!');
