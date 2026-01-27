/**
 * Login Screen
 * @format
 */

import React from 'react';
import { Alert, StyleSheet, View } from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { RootStackParamList } from '../App';
import LoginForm from '../components/LoginForm';

type LoginScreenProps = NativeStackScreenProps<RootStackParamList, 'Login'>;

function LoginScreen({ navigation }: LoginScreenProps) {
  const handleLogin = (username: string, password: string) => {
    Alert.alert('Login Submitted', `Logged in as ${username}`, [
      {
        text: 'OK',
        onPress: () => {
          // Navigate to app selection screen after alert is dismissed
          navigation.navigate('AppSelection');
        },
      },
    ]);
  };

  return (
    <View style={styles.container}>
      <LoginForm onSubmit={handleLogin} />
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
});

export default LoginScreen;
