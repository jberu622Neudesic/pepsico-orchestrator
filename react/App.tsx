/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * @format
 */

import React, { useEffect } from 'react';
import { StatusBar, useColorScheme, Linking } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import LoginScreen from './screens/LoginScreen';
import AppSelectionScreen from './screens/AppSelectionScreen';

export type RootStackParamList = {
  Login: undefined;
  AppSelection: undefined;
};

const Stack = createNativeStackNavigator<RootStackParamList>();

const linking = {
  prefixes: ['flnalauncher://'],
  config: {
    screens: {
      Login: 'login',
      AppSelection: 'app-selection',
    },
  },
};

function App() {
  const isDarkMode = useColorScheme() === 'dark';

  useEffect(() => {
    // Handle deep link when app is opened from a closed state
    const getInitialURL = async () => {
      const url = await Linking.getInitialURL();
      if (url) {
        console.log('App opened with URL:', url);
      }
    };
    getInitialURL();

    // Handle deep link when app is already running
    const subscription = Linking.addEventListener('url', (event) => {
      console.log('Received URL:', event.url);
    });

    return () => {
      subscription.remove();
    };
  }, []);

  return (
    <SafeAreaProvider>
      <StatusBar barStyle={isDarkMode ? 'light-content' : 'dark-content'} />
      <NavigationContainer linking={linking}>
        <Stack.Navigator
          initialRouteName="Login"
          screenOptions={{
            headerShown: false,
          }}
        >
          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="AppSelection" component={AppSelectionScreen} />
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}

export default App;
