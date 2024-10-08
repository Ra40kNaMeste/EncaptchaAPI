import { APIRequestContext, expect } from "@playwright/test"

//Тип пользователя
export type User = {
    id:number,
    email:string,
    role:number
    }

//Тип задания
export type Task = {
  id: number,
  mode: number,
  customer: User|undefined,
  employee: User|undefined,
  working: Date,
  worked: Date,
  solution: string
}

//Расширенные данные пользователя
export type RichUser = User & {
  cache:number,
  customeredTasks: Task[],
  completedTasks: Task[],
}

//Функция регистрации нового пользователя
export async function registration(request:APIRequestContext, email:string, password:string = "qwerty", title:number = 0):Promise<Buffer>{
    const response = await request.post(`/registration?Title=${title}&Email=${email}&Password=${password}`);
    expect(response.ok()).toBeTruthy();
    const token = await response.body();
    expect(token.length).toBeGreaterThan(10);
    return token;
  }

//Функция регистрации нового адимна
export async function adminRegistration(request:APIRequestContext, email:string, password:string = "qwerty", title:number = 3):Promise<Buffer>{
    const response = await request.post(`/registration/super/secret/path?Title=${title}&Email=${email}&Password=${password}&secretKey=${process.env.SUPER_SECRET_KEY}`);
    expect(response.ok()).toBeTruthy();
    const token = await response.body();
    expect(token.length).toBeGreaterThan(10);
    return token;
}

export async function RemoveUser(request: APIRequestContext, token:Buffer){
const response = await request.delete('/user', {
    headers:{
        "Authorization":`Bearer ${token}`
    }
    });
    expect(response.ok()).toBeTruthy();
}

export async function sendFile(request:APIRequestContext, url:string, path:string, token:Buffer):Promise<APIResponse>{
  const form = new FormData();
  form.set('file', new File(['file'], path));
  const response = await request.post(url, {
    headers: {
      Accept: "*/*",
      ContentType: "multipart/form-data",
      Authorization: `Bearer ${token}`
    },
    multipart: form
  });
  expect(response.ok()).toBeTruthy();
  return response;
}

export function delay(ms: number) {
  return new Promise( resolve => setTimeout(resolve, ms) );
}