/**
 * App Selection Screen
 * @format
 */

import React from 'react';
import {
  Alert,
  Linking,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../App';

type AppSelectionScreenProps = NativeStackScreenProps<
  RootStackParamList,
  'AppSelection'
>;

function AppSelectionScreen({}: AppSelectionScreenProps) {
  const handleMauiApp = async () => {
    const deepLink = 'mauiapp://';
    try {
      const canOpen = await Linking.canOpenURL(deepLink);
      if (canOpen) {
        await Linking.openURL(deepLink);
      } else {
        Alert.alert(
          'App Not Available',
          'Maui App is not installed or cannot be opened.',
        );
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to open Maui App');
      console.error('Error opening Maui App:', error);
    }
  };

  const handleSwiftApp = async () => {
    const deepLink = 'FLNA://';
    try {
      const canOpen = await Linking.canOpenURL(deepLink);
      if (canOpen) {
        await Linking.openURL(deepLink);
      } else {
        Alert.alert(
          'App Not Available',
          'FLNA is not installed or cannot be opened.',
        );
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to open FLNA');
      console.error('Error opening FLNA:', error);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Select App</Text>
      <TouchableOpacity
        style={styles.button}
        onPress={handleMauiApp}
        activeOpacity={0.8}
      >
        <Text style={styles.buttonText}>Maui App</Text>
      </TouchableOpacity>
      <TouchableOpacity
        style={styles.button}
        onPress={handleSwiftApp}
        activeOpacity={0.8}
      >
        <Text style={styles.buttonText}>Swift App</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 40,
  },
  button: {
    backgroundColor: '#007AFF',
    borderRadius: 8,
    padding: 16,
    width: '80%',
    maxWidth: 300,
    alignItems: 'center',
    marginBottom: 16,
  },
  buttonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: '600',
  },
});

export default AppSelectionScreen;
