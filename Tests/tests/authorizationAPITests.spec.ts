import { test, expect, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';
import { registration, RemoveUser } from './tools';

//Тест на регистрацию пользователя
test('user registration', async ({ request }) => {
  const token = await registration(request, `testUserRegistration${randomInt(10000)}`);
  await RemoveUser(request, token);
});

//Тест на вход пользователя
test('user login', async ({ request }) => {
  //регистрация пользователя
  const email = `testUserLogin${randomInt(10000)}`;
  const password = `password`;
  await registration(request, email, password);

  //Вход пользователя
  const response = await request.post(`/login?Email=${email}&Password=${password}`);
  expect(response.ok()).toBeTruthy();
  const newtoken = await response.body();
  expect(newtoken.length).toBeGreaterThan(10);

  await RemoveUser(request, newtoken);
});