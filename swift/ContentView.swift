//
//  ContentView.swift
//  FLNA-Swift-App
//
//  Created by Matt Wood on 1/26/26.
//

import SwiftUI
import UIKit

struct ContentView: View {
    @Environment(\.openURL) private var openURL
    @State private var appExists: Bool?
    
    var body: some View {
        VStack {
            Text("Swift App")
            
            if let appExists = appExists {
                Text(appExists ? "✓ App is installed" : "✗ App not found")
                    .foregroundColor(appExists ? .green : .red)
                    .font(.caption)
            }
            
            Button("Open Orchestrator App") {
                // flnalauncher://app-selection
                if let url = URL(string: "flnalauncher://app-selection") {
                    let canOpen = UIApplication.shared.canOpenURL(url)
                    self.appExists = canOpen
                    
                    if canOpen {
                        Task {
                            await openURL(url)
                        }
                    } else {
                        print("Cannot open URL: \(url.absoluteString). Make sure the app is installed and the scheme is registered.")
                    }
                }
            }
            .buttonStyle(.borderedProminent)
            .padding(.top)
        }
        .padding()
        .onAppear {
            // Check if app exists on view appear
            if let url = URL(string: "flnalauncher://app-selection") {
                self.appExists = UIApplication.shared.canOpenURL(url)
            }
        }
    }
}

#Preview {
    ContentView()
}
