
export function convertToJson<T>(value: string |null): T{
    try {
        return JSON.parse (value || '') as T;

    }catch{
        return {} as T;
    }
}

export function splitOnCapitalLetters(value: string): string{
  return  value.split(/(?=[A-Z])/)
    .map((name: string) => name.charAt(0).toUpperCase() + name.slice(1))
    .join(' ');
}

export function splitArray(value: string): string{
    return  value.split(',').join(', ');
  }


