import { test, expect, APIResponse, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';
import { User, registration, RemoveUser } from './tools.ts';

//Тест получения текущего пользователя по токену
test('get user', async({request})=>{
    //регистрация пользователя
    const email = `testUserGet${randomInt(10000)}`;
    const role = 0;
    const token = await registration(request, email, "qwerty", role);
    try
    {
      const response = await request.get('/user',{
        headers:{
          "Authorization":`Bearer ${token}`
        }
      })
      expect(response.ok).toBeTruthy();
      const user = await response.json() as User;
      expect(user.email).toEqual(email);
      expect(user.role).toEqual(role);
    }
    finally
    {
      //Удаление пользователя
      await RemoveUser(request, token);
    }
  });
  
  //Тест получения текущего пользователя без токена
  test('get user not token', async({request})=>{
    //регистрация пользователя
    const email = `testUserGet${randomInt(10000)}`;
    const role = 0;
    const token = await registration(request, email, "qwerty", role);
    try
    {
      const response = await request.get('/user',)
      expect(response.ok()).toBeFalsy();
    }
    finally
    {
      //Удаление пользователя
      await RemoveUser(request, token);
    }
  });

//Тест смены роли с employeer на customer
test('user post. Employeer change to custpomer', async({request})=>await testRole(`Employeer2Customer${randomInt(10000)}`, request, 0, 1));

//Тест смены роли с customer на employeer
test('user post. Customer change to employeer', async({request})=>await testRole(`Customer2Employeer${randomInt(10000)}`, request, 1, 0));

//Тест смены роли с customer на admin
test('user post. Customer change to admin', async({request})=>await testRole(`Customer2Admin${randomInt(10000)}`, request, 1, 2, false));

//Универсальная функция для тестирования изменения роли текущего пользователя по токену
async function testRole(email:string, request:APIRequestContext, role:number, newRole:number, succesfullRef:boolean = true):Promise<undefined>
{
  //регистрация пользователя
  const token = await registration(request, email, "qwerty", role);
  try
  {
    
    //Изменение роли
    {
        const response = await request.put(`/user?role=${newRole}`,{
          headers:{
            "Authorization":`Bearer ${token}`
          }
        });
        expect(response.ok() === succesfullRef).toBeTruthy();
      }
      
      //Проверка роли
      {
        const response = await request.get('user',{
          headers:{
            "Authorization":`Bearer ${token}`
          }
        });
        expect(response.ok()).toBeTruthy();
        const user = await response.json() as User;
        expect((user.role == newRole)==succesfullRef).toBeTruthy();
      }
  }
  
  finally
  {
    await RemoveUser(request, token);
  }
};