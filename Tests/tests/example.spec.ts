import { test, expect, APIResponse, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';

//Тест на регистрацию пользователя
test('user registration', async ({ request }) => {
  const token = await registration(request, `testUserRegistration${randomInt(10000)}`);
  {
    //Удаление пользователя
    const response = await request.delete('/user', {
      headers:{
        "Authorization":`Bearer ${token}`
      }
    });
    expect(response.ok()).toBeTruthy();
  }
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

  {
    //Удаление пользователя
    const response = await request.delete('/user', {
      headers:{
        "Authorization":`Bearer ${newtoken}`
      }
    });
    expect(response.ok()).toBeTruthy();
  }
});

//Функция регистрации нового пользователя
async function registration(request:APIRequestContext, email:string, password:string = "qwerty", title:number = 0):Promise<Buffer>{
  const response = await request.post(`/registration?Title=${title}&Email=${email}&Password=${password}`);
  expect(response.ok()).toBeTruthy();
  const token = await response.body();
  expect(token.length).toBeGreaterThan(10);
  return token;
}