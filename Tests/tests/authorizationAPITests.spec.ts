import { test, expect, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';
import { adminRegistration, registration, RemoveUser } from './tools';

//Тест на регистрацию пользователя
test('user registration', async ({ request }) => {
  const token = await registration(request, `testUserRegistration${randomInt(10000)}`);
  await RemoveUser(request, token);
});

//Тест на регистрацию админа пользователя
test('bed admin registration', async ({ request }) => {
  const title = 2;
  const email = `testBadAdminRegistration${randomInt(10000)}`;
  const response = await request.post(`/registration?Title=${title}&Email=${email}&Password=qwerty`);
  expect(response.ok()).toBeFalsy();
});

//Тест на регистрацию 2-х пользователей с одинаковым емейлом
test('two users registration', async ({ request }) => {
  const email = `TwoUsersRegistration${randomInt(10000)}`;
  const token = await registration(request, email);
  const response = await request.post(`/registration?Title=0&Email=${email}&Password=qwerty`);
  expect(response.ok()).toBeFalsy();
  await RemoveUser(request, token);
});

//Тест регистрации супер админа
test('admin registration',async ({request})=>{
  const token = await adminRegistration(request, `testUserRegistration${randomInt(10000)}`);
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