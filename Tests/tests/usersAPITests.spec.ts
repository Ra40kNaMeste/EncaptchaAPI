import { test, expect, APIResponse, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';
import { registration, RemoveUser, User, RichUser } from './tools';

//Тест получения всех пользователей
test('get users test', async({request})=>{

    //Список емейлов
    const emails:string[] = [];
    //Аналогичный список токенов для удаления пользователей
    const tokens:Buffer[] = [];
    
    //Регистрация пользователей
    for(let i=0; i<10; i++){
      const email = `testGetUsers${randomInt(10000)}`;
      const token = await registration(request, email, "qwerty");
      emails.push(email);
      tokens.push(token)
    }
  try
  {
    //Получение массива зарегистрированных пользователей
    const response = await request.get("users");
    expect(response.ok()).toBeTruthy();
    const users = await response.json() as User[];
    
    //Проверяем добавление всех пользователей
    const findedUsers = emails.filter(i=>users.some(u=>u.email == i));
    expect(findedUsers.length).toEqual(emails.length);
  }

  finally{
    //Удаление пользователей
    for(const token of tokens){
      await RemoveUser(request, token);
    }
  }
    
    
  });

//Тест получения пользователя без авторизации
test('get user by ip. No authorization', async ({request})=>{
  const email = `GetUserByIPNoAuthorization${randomInt(10000)}`;
  const token = await registration(request, email);

 
  try
  {
    //Получение Ip пользователя
    let userIp:number;
    {
      const response = await request.get('user', {
        headers: {
          "Authorization": `Bearer ${token}`
        }
      });
      const userData = await response.json() as User;
      userIp = userData.id
    }

    //Получение пользователя по Ip и проверка его email
    {
      const response = await request.get(`user/${userIp}`);
      expect(response.ok()).toBeTruthy();
      const user = await response.json() as User;
      expect(user.email, email);
    }
  }
  finally{
    await RemoveUser(request, token);
  }
});

//Надо будет потом дописать
//Тест получения пользователя c авторизацией
test('get user by ip. With authorization', async ({request})=>{
  const email = `GetUserByIPWithAuthorization${randomInt(10000)}`;
  const token = await registration(request, email);
  try{
//Получение Ip пользователя
    let userId:number;
    {
      const response = await request.get('user', {
        headers: {
          "Authorization": `Bearer ${token}`
        }
      });
      const userData = await response.json() as User;
      userId = userData.id
    }

    //Получение пользователя по Ip и проверка его email
    {
      const response = await request.get(`user/${userId}`);
      expect(response.ok()).toBeTruthy();
      const user = await response.json() as RichUser;
      expect(user.email, email);
    }
  }
  
finally{
  //Удаление пользователя
  await RemoveUser(request, token);
}

});

