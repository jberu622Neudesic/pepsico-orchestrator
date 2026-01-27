//
//  FLNA_Swift_AppApp.swift
//  FLNA-Swift-App
//
//  Created by Matt Wood on 1/26/26.
//

import SwiftUI

@main
struct FLNA_Swift_AppApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
                .onOpenURL { url in
                    // Handle URL scheme: flna://
                    print("Received URL: \(url)")
                    // Add your URL handling logic here
                }
        }
    }
}
