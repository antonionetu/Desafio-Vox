export interface IToken {
    id: number;
    usuarioId: number;
    tipo: string;
}

export class Token {
    private tokenString: string | null;

    constructor() {
        this.tokenString = sessionStorage.getItem('token');
    }

    public asString(): string | null {
        return this.tokenString;
    }

    public asObject(): IToken | null {
        if (!this.tokenString) return null;

        try {
            const payloadBase64 = this.tokenString.split('.')[1];
            const payloadJson = atob(payloadBase64);
            const payload = JSON.parse(payloadJson);

            return {
                id: payload.id,
                usuarioId: payload.usuarioId,
                tipo: payload.tipo,
            };
        } catch (error) {
            console.error('Erro ao decodificar token:', error);
            return null;
        }
    }
}
