/******************************************************************************
 * (C) Copyright 2012 Atech-Automotive
 * FILE NAME:    AES.c
 * DESCRIPTION:  此算法只提供了128bit的加密级别
 *               默认密钥: 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88
 *                         0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
 *               AES算法，基本变换包括SubBytes（字节替代）、ShiftRows（行移位）、
 *                        MixColumns（列混淆）、AddRoundKey(轮密钥加）
 *               AES解密算法与加密不同，基本运算中除了AddRoundKey（轮密钥加）
 *               不变外，其余的都需要进行逆变换，即:
 *               InvSubBytes（逆字节替代）、InvShiftRows（逆行移位）、
 *               InvMixColumns（逆列混淆） 
 *               
 *               aes.Mode = CipherMode.ECB;
 *               aes.Padding = PaddingMode.Zeros;
 *               aes.KeySize = 128;          
 * 
 * DATE BEGUN:   2012/10/18
 * BY:           hui.pang
 * PRODUCT NAME:
 * APPLICATION:
 * TARGET H/W:
 * DOC REF:
 *****************************************************************************
 */



/*****************************************************************************
** #include 
*****************************************************************************/

#include "app_vcu_encrypt.h"

/*****************************************************************************
** #define
*****************************************************************************/
#define BPOLY 0x1b    /* Lower 8 bits of (x^8+x^4+x^3+x+1),ie.(x^4+x^3+x+1). */
#define BLOCK_SIZE 16 /* Block size in number of bytes. */
     
#define KEY_BITS 128  /* Use AES128. */
#define ROUNDS 10     /* Number of rounds. */
#define KEY_LENGTH 16 /* Key length in number of bytes. */

/*****************************************************************************
** typedef
*****************************************************************************/


/*****************************************************************************
** global variable
*****************************************************************************/


/*****************************************************************************
** static variables
*****************************************************************************/
static UINT8 u8_block1[ 256 ]; /* Workspace 1. */
static UINT8 u8_block2[ 256 ]; /* Worksapce 2. */

static UINT8  s_u8_sbox_buf[256] = { 0 };

/* AES Secret Key */
static UINT8 s_u8_secret_key[16] = {0x00, 0x11, 0x22, 0x33, 0x44, 0x55,
                                    0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB,
                                    0xCC, 0xDD, 0xEE, 0xFF};

static UINT8 * p_u8_pow_tbl; /* Final location of exponentiation lookup table. */
static UINT8 * p_u8_log_tbl;    /* Final location of logarithm lookup table. */
static UINT8 * p_u8_sbox;       /* Final location of s-box. */
static UINT8 * p_u8_sbox_inv;   /* Final location of inverse s-box. */
static UINT8 * p_u8_expand_key; /* Final location of expanded p_u8_key. */

/*****************************************************************************
** static constants
*****************************************************************************/


/*****************************************************************************
** static function prototypes
*****************************************************************************/


/*****************************************************************************
** function prototypes
*****************************************************************************/


/****************************************************************************/
/****************************************************************************/
/**
 * Function Name: static void Calc_Pow_Log( UINT8 * p_u8_pow_tbl, 
 *                                          UINT8 * p_u8_log_tbl )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Calc_Pow_Log( UINT8 * p_u8_pow_tbl, UINT8 * p_u8_log_tbl )
{
    UINT8 u8_i = 0;
    UINT8 t = 1;
    UINT8 temp1 = 0;                
    UINT8 temp2 = 0;

    do {
        /* Use 0x03 as root for exponentiation and logarithms. */
        p_u8_pow_tbl[u8_i] = t;
        p_u8_log_tbl[t] = u8_i;
        u8_i++;

        /* Muliply t by 3 in GF(2^8). */
        t ^= (t << 1) ^ (t & 0x80 ? BPOLY : 0);
    } while( t != 1 ); /* Cyclic properties ensure that u8_i < 255. */

    p_u8_pow_tbl[255] = p_u8_pow_tbl[0]; /* 255 = '-0', 254 = -1, etc. */
} 

/****************************************************************************/
/**
 * Function Name: static void Calc_SBox( UINT8 * p_u8_sbox )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Calc_SBox( UINT8 * p_u8_sbox )
{
    UINT8  u8_i = 0;
    UINT8  u8_rot = 0;
    UINT8  u8_temp = 0;
    UINT8  u8_result = 0;

    /* Fill all entries of p_u8_sbox[]. */
    do {
        /* Inverse in GF(2^8). */
        if( u8_i > 0 ) 
        {
            u8_temp = p_u8_pow_tbl[ 255 - p_u8_log_tbl[u8_i] ];
        } 
        else 
        {
            u8_temp = 0;
        }

        /* Affine transformation in GF(2). */
        /* Start with adding u8_key_temp vector in GF(2). */
        u8_result = u8_temp ^ 0x63; 
        
        for( u8_rot = 0; u8_rot < 4; u8_rot++ ) 
        {
            /* Rotate left. */
            u8_temp = (u8_temp << 1) | (u8_temp >> 7);

            /* Add rotated UINT8 in GF(2). */
            u8_result ^= u8_temp;
        }

        /* Put u8_result in table. */
        p_u8_sbox[u8_i] = u8_result;
    } while( ++u8_i != 0 );
}	

/****************************************************************************/
/**
 * Function Name: static void Calc_SBox_Inv( UINT8 * p_u8_sbox, 
 *                                           UINT8 * p_u8_sbox_inv )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Calc_SBox_Inv( UINT8 * p_u8_sbox, UINT8 * p_u8_sbox_inv )
{
    UINT8  u8_i = 0;
    UINT8  u8_j = 0;

    /* Iterate through all elements in p_u8_sbox_inv using  u8_i. */
    do {
        /* Search through p_u8_sbox using j. */
        do {
            /* Check if current j is the inverse of current u8_i. */
            if( p_u8_sbox[ u8_j ] == u8_i ) 
            {
                /* If so, set sBoxInc and indicate search finished. */
                p_u8_sbox_inv[ u8_i ] = u8_j;
                u8_j = 255;
            }
        } while( ++u8_j != 0 );
    } while( ++u8_i != 0 );
}

/****************************************************************************/
/**
 * Function Name: static void Cycle_Left( UINT8 * p_u8_row )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Cycle_Left( UINT8 * p_u8_row )
{
    /* Cycle 4 bytes in an array left once. */
    UINT8  u_8_temp = p_u8_row[0];
    
    p_u8_row[0] = p_u8_row[1];
    p_u8_row[1] = p_u8_row[2];
    p_u8_row[2] = p_u8_row[3];
    p_u8_row[3] = u_8_temp;
}

/****************************************************************************/
/**
 * Function Name: static void Inv_Mix_column( UINT8 * p_u8_column )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Inv_Mix_column( UINT8 * p_u8_column )
{
    UINT8 u8_r0 = 0;
    UINT8 u8_r1 = 0;
    UINT8 u8_r2 = 0;
    UINT8 u8_r3 = 0;

    u8_r0 = p_u8_column[1] ^ p_u8_column[2] ^ p_u8_column[3];
    u8_r1 = p_u8_column[0] ^ p_u8_column[2] ^ p_u8_column[3];
    u8_r2 = p_u8_column[0] ^ p_u8_column[1] ^ p_u8_column[3];
    u8_r3 = p_u8_column[0] ^ p_u8_column[1] ^ p_u8_column[2];

    p_u8_column[0] = (p_u8_column[0] << 1) ^ (p_u8_column[0] & 0x80 ? BPOLY : 0);
    p_u8_column[1] = (p_u8_column[1] << 1) ^ (p_u8_column[1] & 0x80 ? BPOLY : 0);
    p_u8_column[2] = (p_u8_column[2] << 1) ^ (p_u8_column[2] & 0x80 ? BPOLY : 0);
    p_u8_column[3] = (p_u8_column[3] << 1) ^ (p_u8_column[3] & 0x80 ? BPOLY : 0);

    u8_r0 ^= p_u8_column[0] ^ p_u8_column[1];
    u8_r1 ^= p_u8_column[1] ^ p_u8_column[2];
    u8_r2 ^= p_u8_column[2] ^ p_u8_column[3];
    u8_r3 ^= p_u8_column[0] ^ p_u8_column[3];

    p_u8_column[0] = (p_u8_column[0] << 1) ^ (p_u8_column[0] & 0x80 ? BPOLY : 0);
    p_u8_column[1] = (p_u8_column[1] << 1) ^ (p_u8_column[1] & 0x80 ? BPOLY : 0);
    p_u8_column[2] = (p_u8_column[2] << 1) ^ (p_u8_column[2] & 0x80 ? BPOLY : 0);
    p_u8_column[3] = (p_u8_column[3] << 1) ^ (p_u8_column[3] & 0x80 ? BPOLY : 0);

    u8_r0 ^= p_u8_column[0] ^ p_u8_column[2];
    u8_r1 ^= p_u8_column[1] ^ p_u8_column[3];
    u8_r2 ^= p_u8_column[0] ^ p_u8_column[2];
    u8_r3 ^= p_u8_column[1] ^ p_u8_column[3];

    p_u8_column[0] = (p_u8_column[0] << 1) ^ (p_u8_column[0] & 0x80 ? BPOLY : 0);
    p_u8_column[1] = (p_u8_column[1] << 1) ^ (p_u8_column[1] & 0x80 ? BPOLY : 0);
    p_u8_column[2] = (p_u8_column[2] << 1) ^ (p_u8_column[2] & 0x80 ? BPOLY : 0);
    p_u8_column[3] = (p_u8_column[3] << 1) ^ (p_u8_column[3] & 0x80 ? BPOLY : 0);

    p_u8_column[0] ^= p_u8_column[1] ^ p_u8_column[2] ^ p_u8_column[3];
    u8_r0 ^= p_u8_column[0];
    u8_r1 ^= p_u8_column[0];
    u8_r2 ^= p_u8_column[0];
    u8_r3 ^= p_u8_column[0];

    p_u8_column[0] = u8_r0;
    p_u8_column[1] = u8_r1;
    p_u8_column[2] = u8_r2;
    p_u8_column[3] = u8_r3;
}

/****************************************************************************/
/**
 * Function Name: static UINT8 Multiply( UINT8 u8_num, UINT8 u8_factor )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static UINT8 Multiply( UINT8 u8_num, UINT8 u8_factor )
{
    UINT8 mask = 1;
    UINT8 u8_result = 0;

    while( mask != 0 ) 
    {
        /* Check bit of u8_factor given by mask. */
        if( mask & u8_factor ) 
        {
            /* Add current multiple of u8_num in GF(2). */
            u8_result ^= u8_num;
        }

        /* Shift mask p_u8_to indicate next bit. */
        mask <<= 1;

        /* Double u8_num. */
        u8_num = (u8_num << 1) ^ (u8_num & 0x80 ? BPOLY : 0);
    }

    return u8_result;
}

/****************************************************************************/
/**
 * Function Name: static UINT8 Dot_Product( UINT8 * p_u8_vectou8_r1, 
 *                                          UINT8 * p_u8_vectou8_r2 )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static UINT8 Dot_Product( UINT8 * p_u8_vectou8_r1, UINT8 * p_u8_vectou8_r2 )
{
    UINT8 u8_result = 0;

    u8_result ^= Multiply( *p_u8_vectou8_r1++, *p_u8_vectou8_r2++ );
    u8_result ^= Multiply( *p_u8_vectou8_r1++, *p_u8_vectou8_r2++ );
    u8_result ^= Multiply( *p_u8_vectou8_r1++, *p_u8_vectou8_r2++ );
    u8_result ^= Multiply( *p_u8_vectou8_r1  , *p_u8_vectou8_r2   );

    return u8_result;
}

/****************************************************************************/
/**
 * Function Name: static void Mixp_Column( UINT8 * p_u8_column )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Mixp_Column( UINT8 * p_u8_column )
{
    /* Prepare first u8_row of matrix twice, 
       p_u8_to eliminate need for cycling. */
    UINT8  u8_row[8] = 
    {
        0x02, 0x03, 0x01, 0x01,
        0x02, 0x03, 0x01, 0x01
    }; 

    UINT8  u8_result[4];

    // Take dot products of each matrix u8_row and the p_u8_column vector.
    u8_result[0] = Dot_Product( u8_row + 0, p_u8_column );
    u8_result[1] = Dot_Product( u8_row + 3, p_u8_column );
    u8_result[2] = Dot_Product( u8_row + 2, p_u8_column );
    u8_result[3] = Dot_Product( u8_row + 1, p_u8_column );

    // Copy temporary u8_result p_u8_to original p_u8_column.
    p_u8_column[0] = u8_result[0];
    p_u8_column[1] = u8_result[1];
    p_u8_column[2] = u8_result[2];
    p_u8_column[3] = u8_result[3];
}


/****************************************************************************/
/**
 * Function Name: static void Sub_Bytes( UINT8 * p_u8_bytes, UINT8 u8_count )
 * Description: 非线性的字节替代，单独处理每个字节
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Sub_Bytes( UINT8 * p_u8_bytes, UINT8 u8_count )
{
    /* Substitute every UINT8 in p_u8_state. */
    do {
        *p_u8_bytes = p_u8_sbox[ *p_u8_bytes ]; 
        p_u8_bytes++;
    } while( --u8_count );
}

/****************************************************************************/
/**
 * Function Name: static void Inv_Sub_Bytes_And_XOR( UINT8 * p_u8_bytes, 
 *                                                   UINT8 * p_u8_key, 
 *                                                   UINT8 u8_count )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Inv_Sub_Bytes_And_XOR( UINT8 * p_u8_bytes, UINT8 * p_u8_key, 
                                   UINT8 u8_count )
{
    do {
        /* Inverse substitute every UINT8 in p_u8_state and add p_u8_key. */
        //*bytes = p_u8_sbox_inv[ *bytes ] ^ *p_u8_key; 
        
        /* Use u8_block2 directly. Increases speed. */
        *p_u8_bytes = u8_block2[ *p_u8_bytes ] ^ *p_u8_key; 
        p_u8_bytes++;
        p_u8_key++;
    } while( --u8_count );
}

/****************************************************************************/
/**
 * Function Name: static void Inv_Shift_Rows( UINT8 * p_u8_state )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Inv_Shift_Rows( UINT8 * p_u8_state )
{
    UINT8 u8_temp = 0;

    /* Note: State is arranged p_u8_column by p_u8_column. */

    /* Cycle second u8_row right one time. */
    u8_temp = p_u8_state[ 1 + 3 * 4 ];
    p_u8_state[ 1 + 3 * 4 ] = p_u8_state[ 1 + 2 * 4 ];
    p_u8_state[ 1 + 2 * 4 ] = p_u8_state[ 1 + 1 * 4 ];
    p_u8_state[ 1 + 1 * 4 ] = p_u8_state[ 1 + 0 * 4 ];
    p_u8_state[ 1 + 0 * 4 ] = u8_temp;

    /* Cycle third u8_row right two times. */
    u8_temp = p_u8_state[ 2 + 0 * 4 ];
    p_u8_state[ 2 + 0 * 4 ] = p_u8_state[ 2 + 2 * 4 ];
    p_u8_state[ 2 + 2 * 4 ] = u8_temp;
    u8_temp = p_u8_state[ 2 + 1*4 ];
    p_u8_state[ 2 + 1 * 4 ] = p_u8_state[ 2 + 3 * 4 ];
    p_u8_state[ 2 + 3 * 4 ] = u8_temp;

    /* Cycle fourth u8_row right three times, ie. left once. */
    u8_temp = p_u8_state[ 3 + 0 * 4 ];
    p_u8_state[ 3 + 0 * 4 ] = p_u8_state[ 3 + 1 * 4 ];
    p_u8_state[ 3 + 1 * 4 ] = p_u8_state[ 3 + 2 * 4 ];
    p_u8_state[ 3 + 2 * 4 ] = p_u8_state[ 3 + 3 * 4 ];
    p_u8_state[ 3 + 3 * 4 ] = u8_temp;
}

/****************************************************************************/
/**
 * Function Name: static void Shift_Rows( UINT8 * p_u8_state )
 * Description: 行移位变换完成基于行的循环位移操作
 *              即行移位变换作用于行上，第0行不变，第1行循环左移1个字节，
 *              第2行循环左移2个字节，第3行循环左移3个字节。
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Shift_Rows( UINT8 * p_u8_state )
{
    UINT8 u8_temp = 0;

    /* Note: State is arranged p_u8_column by p_u8_column. */

    /* Cycle second u8_row left one time. */
    u8_temp = p_u8_state[ 1 + 0*4 ];
    p_u8_state[ 1 + 0*4 ] = p_u8_state[ 1 + 1*4 ];
    p_u8_state[ 1 + 1*4 ] = p_u8_state[ 1 + 2*4 ];
    p_u8_state[ 1 + 2*4 ] = p_u8_state[ 1 + 3*4 ];
    p_u8_state[ 1 + 3*4 ] = u8_temp;

    /* Cycle third u8_row left two times. */
    u8_temp = p_u8_state[ 2 + 0*4 ];
    p_u8_state[ 2 + 0*4 ] = p_u8_state[ 2 + 2*4 ];
    p_u8_state[ 2 + 2*4 ] = u8_temp;
    u8_temp = p_u8_state[ 2 + 1*4 ];
    p_u8_state[ 2 + 1*4 ] = p_u8_state[ 2 + 3*4 ];
    p_u8_state[ 2 + 3*4 ] = u8_temp;

    /* Cycle fourth u8_row left three times, ie. right once. */
    u8_temp = p_u8_state[ 3 + 3 * 4 ];
    p_u8_state[ 3 + 3 * 4 ] = p_u8_state[ 3 + 2 * 4 ];
    p_u8_state[ 3 + 2 * 4 ] = p_u8_state[ 3 + 1 * 4 ];
    p_u8_state[ 3 + 1 * 4 ] = p_u8_state[ 3 + 0 * 4 ];
    p_u8_state[ 3 + 0 * 4 ] = u8_temp;
}

/****************************************************************************/
/**
 * Function Name: static void Inv_Mix_columns( UINT8 * p_u8_state )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Inv_Mix_columns( UINT8 * p_u8_state )
{
    Inv_Mix_column( p_u8_state + 0 * 4 );
    Inv_Mix_column( p_u8_state + 1 * 4 );
    Inv_Mix_column( p_u8_state + 2 * 4 );
    Inv_Mix_column( p_u8_state + 3 * 4 );
}

/****************************************************************************/
/**
 * Function Name: static void Mix_Columns( UINT8 * p_u8_state )
 * Description: 列混淆变换
 *              b(x) = (03·x3 + 01·x2 + 01·x + 02)·a(x) mod(x4 + 1)
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Mix_Columns( UINT8 * p_u8_state )
{
    Mixp_Column( p_u8_state + 0 * 4 );
    Mixp_Column( p_u8_state + 1 * 4 );
    Mixp_Column( p_u8_state + 2 * 4 );
    Mixp_Column( p_u8_state + 3 * 4 );
}

/****************************************************************************/
/**
 * Function Name: static void XOR_Bytes( UINT8 * p_u8_bytes1, 
 *                                       UINT8 * p_u8_bytes2, 
 *                                       UINT8 u8_count )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void XOR_Bytes( UINT8 * p_u8_bytes1, UINT8 * p_u8_bytes2, UINT8 u8_count )
{
    do {
        *p_u8_bytes1 ^= * p_u8_bytes2; /* Add in GF(2), ie. XOR. */
        p_u8_bytes1++;
        p_u8_bytes2++;
    } while( --u8_count );
}

/****************************************************************************/
/**
 * Function Name: static void Copy_Bytes( UINT8 * p_u8_to, UINT8 * p_u8_from, 
 *                                        UINT8 u8_count )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Copy_Bytes( UINT8 * p_u8_to, UINT8 * p_u8_from, UINT8 u8_count )
{
    do {
        * p_u8_to = * p_u8_from;
        p_u8_to++;
        p_u8_from++;
    } while( --u8_count );
}

/****************************************************************************/
/**
 * Function Name: static void Key_Expansion( UINT8 * p_u8_expand_key )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Key_Expansion( UINT8 * p_u8_expand_key )
{
    UINT8  u8_temp[4] = { 0 };
    UINT8  u8_i = 0;
    UINT8  u8_rcon[4] = { 0x01, 0x00, 0x00, 0x00 }; /* Round constant. */

    UINT8  *p_u8_key = (void*)0;

    p_u8_key = s_u8_secret_key;

    ////////////////////////////////////////////
     
    /* Copy p_u8_key p_u8_to start of expanded p_u8_key. */
    u8_i = KEY_LENGTH;

    do {
        *p_u8_expand_key = *p_u8_key;
        p_u8_expand_key++;
        p_u8_key++;
    } while( --u8_i );

    /* Prepare last 4 bytes of p_u8_key in u8_temp. */
    p_u8_expand_key -= 4;
    u8_temp[0] = *(p_u8_expand_key++);
    u8_temp[1] = *(p_u8_expand_key++);
    u8_temp[2] = *(p_u8_expand_key++);
    u8_temp[3] = *(p_u8_expand_key++);

    /* Expand p_u8_key. */
    u8_i = KEY_LENGTH;

    while( u8_i < BLOCK_SIZE*(ROUNDS+1) ) 
    {
        /* Are we at the start of u8_key_temp multiple of the p_u8_key size */
        if( (u8_i % KEY_LENGTH) == 0 ) 
        {
            Cycle_Left( u8_temp );             /* Cycle left once.       */
            Sub_Bytes( u8_temp, 4 );          /* Substitute each UINT8. */
            XOR_Bytes( u8_temp, u8_rcon, 4 ); /* Add constant in GF(2). */
            *u8_rcon = (*u8_rcon << 1) ^ (*u8_rcon & 0x80 ? BPOLY : 0);
        }

        /* Add bytes in GF(2) one KEY_LENGTH away. */
        XOR_Bytes( u8_temp, p_u8_expand_key - KEY_LENGTH, 4 );

        /* Copy u8_result p_u8_to current 4 bytes. */
        *(p_u8_expand_key++) = u8_temp[ 0 ];
        *(p_u8_expand_key++) = u8_temp[ 1 ];
        *(p_u8_expand_key++) = u8_temp[ 2 ];
        *(p_u8_expand_key++) = u8_temp[ 3 ];

        u8_i += 4; /* Next 4 bytes. */
    }	
}

/****************************************************************************/
/**
 * Function Name: static void Inv_Cipher( UINT8 * p_u8_block, 
 *                                        UINT8 * p_u8_expand_key )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Inv_Cipher( UINT8 * p_u8_block, UINT8 * p_u8_expand_key )
{
    UINT8 u8_round = ROUNDS - 1;
    p_u8_expand_key += BLOCK_SIZE * ROUNDS;

    XOR_Bytes( p_u8_block, p_u8_expand_key, 16 );
    p_u8_expand_key -= BLOCK_SIZE;

    do {
        Inv_Shift_Rows( p_u8_block );
        Inv_Sub_Bytes_And_XOR( p_u8_block, p_u8_expand_key, 16 );
        p_u8_expand_key -= BLOCK_SIZE;
        Inv_Mix_columns( p_u8_block );
    } while( --u8_round );

    Inv_Shift_Rows( p_u8_block );
    Inv_Sub_Bytes_And_XOR( p_u8_block, p_u8_expand_key, 16 );
}

/****************************************************************************/
/**
 * Function Name: static void Cipher( UINT8 * p_u8_block, UINT8 * p_u8_expand_key )	 
 * Description: 完成一个块(16字节，128bit)的加密
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void Cipher( UINT8 * p_u8_block, UINT8 * p_u8_expand_key )	   
{
    UINT8 u8_round = ROUNDS-1;

    XOR_Bytes( p_u8_block, p_u8_expand_key, 16 );
    p_u8_expand_key += BLOCK_SIZE;

    do {
        Sub_Bytes( p_u8_block, 16 );
        Shift_Rows( p_u8_block );
        Mix_Columns( p_u8_block );
        XOR_Bytes( p_u8_block, p_u8_expand_key, 16 );
        p_u8_expand_key += BLOCK_SIZE;
    } while( --u8_round );

    Sub_Bytes( p_u8_block, 16 );
    Shift_Rows( p_u8_block );
    XOR_Bytes( p_u8_block, p_u8_expand_key, 16 );
}

/****************************************************************************/
/**
 * Function Name: void AES_Init( void )
 * Description: none
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void AES_Init( void  )
{
    UINT8 u8_i = 0;
    UINT8 *p_u8_temp_buf = s_u8_sbox_buf;

    p_u8_pow_tbl = u8_block1;
    p_u8_log_tbl = u8_block2;
    Calc_Pow_Log( p_u8_pow_tbl, p_u8_log_tbl );

    p_u8_sbox = p_u8_temp_buf;
    Calc_SBox( p_u8_sbox );
    
    /* 至此u8_block1用来存贮密码表 */
    p_u8_expand_key = u8_block1;  
    Key_Expansion( p_u8_expand_key );

    /* Must be u8_block2. u8_block2至此开始只用来存贮SBOXINV */
    p_u8_sbox_inv = u8_block2; 
    Calc_SBox_Inv( p_u8_sbox, p_u8_sbox_inv );
}	

/*  */
/****************************************************************************/
/**
 * Function Name: static void AES_Decrypt( UINT8 * p_u8_buffer, 
 *                                         UINT8 * p_u8_chain_block )
 * Description: 对一个16字节块解密,参数p_u8_buffer是解密密缓存，
 *              p_u8_chain_block是要解密的块
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void AES_Decrypt( UINT8 * p_u8_buffer, UINT8 * p_u8_chain_block )
{
    //UINT8  u8_temp[ BLOCK_SIZE ];

    // Copy_Bytes( u8_temp, p_u8_buffer, BLOCK_SIZE ); 
    
    Copy_Bytes(p_u8_buffer,p_u8_chain_block, BLOCK_SIZE);
    Inv_Cipher( p_u8_buffer, p_u8_expand_key );
    /* XOR_Bytes( p_u8_buffer, p_u8_chain_block, BLOCK_SIZE ); */
    Copy_Bytes( p_u8_chain_block, p_u8_buffer, BLOCK_SIZE );
}


/****************************************************************************/
/**
 * Function Name: static void AES_Encrypt( UINT8 * p_u8_buffer, 
 *                                         UINT8 * p_u8_chain_block )
 * Description: 对一个16字节块完成加密，参数p_u8_buffer是加密缓存，
 *              p_u8_chain_block是要加密的块
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
static void AES_Encrypt( UINT8 * p_u8_buffer, UINT8 * p_u8_chain_block )
{
    Copy_Bytes( p_u8_buffer, p_u8_chain_block, BLOCK_SIZE );
    // XOR_Bytes( p_u8_buffer, p_u8_chain_block, BLOCK_SIZE ); 
    Cipher( p_u8_buffer, p_u8_expand_key );
    Copy_Bytes( p_u8_chain_block, p_u8_buffer, BLOCK_SIZE );
}

/****************************************************************************/
/**
 * Function Name: ERR_T AES_Block_Handle( AES_HANDLE_E b_direct, 
 *                                         UINT8 *p_u8_chiper_data_buf, 
 *                                         UINT8 u8_data_len )
 * Description: Decrypt or Encrypt
 *              分配的p_u8_chiper_data_buf数组的大小必须是16的整数倍，
 *              u8_data_len可以不是16整数倍，但其实内部也是按整数倍来计算的。
 *              比如要计算的数组个数为15，则最后一位补零。
 *              
 *              注：1.请尽快不要在中断中使用该函数，不是可重入函数。
 *                  2.函数会直接在输入的数组中做修改，故如果需要输入保存的话
 *                    请提前保存。
 *                  3.如加密明文是18 bit，则密文应为32 bit，但函数内部不会自
 *                    动分配多余的空间，故输入的数组必须已经为32 bit，多余用
 *                    0填充。
 *              
 *              aes.Mode = CipherMode.ECB;
 *              aes.Padding = PaddingMode.Zeros;
 *              aes.KeySize = 128;
 *
 * Param arg1 b_direct: Decrypt or Encrypt
 *       arg2 p_u8_chiper_data_buf: Plaintext(Ciphertext)
 *       arg3 u8_data_len: the length of Plaintext(Ciphertext)
 *       arg4 p_u8_secret_key: Secret Key Text
 * Return:  - Error code, possible codes:
 *            AES_ERR_OK    - OK
 *            AES_ERR_VALUE - Parameter of incorrect value. 
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
ERR_T AES_Block_Handle( AES_HANDLE_E b_direct, UINT8 *p_u8_chiper_data_buf, 
                         UINT8 u8_data_len )
{               
    UINT8  u8_i = 0;       
    UINT8  u8_blocks = 0;

    UINT8  p_u8_temp_buf[16] = { 0 };

    if( b_direct != AES_ENCRYPT && b_direct != AES_DECRYPT )
    {
        return AES_ERR_VALUE;
    }

    if( p_u8_chiper_data_buf == (void*)0 )
    {
        return AES_ERR_VALUE;
    }

//    AES_Init(s_u8_sbox_buf);/* 初始化 */

    if( b_direct == AES_DECRYPT )  /* 解密 */
    {
        if(u8_data_len % 16 != 0)
        {  
            u8_blocks = u8_data_len / 16 + 1;
            /* 不足16字节的块补零处理 */
        }
        else
        {
            u8_blocks = u8_data_len / 16;
        }

        for(u8_i = 0; u8_i < u8_blocks; u8_i++)
        {

            AES_Decrypt(p_u8_temp_buf, p_u8_chiper_data_buf /*+ 4*/ + 16 * u8_i);
        }
//        (void)memmove(p_u8_chiper_data_buf, p_u8_chiper_data_buf /*+ 4*/, u8_data_len);

    }
    else  /* 加密 */
    {
        if(u8_data_len % 16 != 0)
        {  
            u8_blocks = u8_data_len / 16 + 1;
            /* 不足16字节的块补零处理 */
        }
        else
        {
            u8_blocks = u8_data_len / 16;
        }

        for(u8_i = 0; u8_i < u8_blocks; u8_i++)
        {

            AES_Encrypt(p_u8_temp_buf, p_u8_chiper_data_buf /*+ 4*/ + 16 * u8_i);
        }       
    }
    
    return AES_ERR_OK;

}

/****************************************************************************/
/**
 * Function Name: ERR_T AES_Change_Secret_Key( const UINT8 * p_u8_secret_key )
 * Description: 
 *             更好密钥，请尽快不要在中断中使用该函数，不是可重入函数
 *             在进行AES加解密之前需执行该函数，并传入一个16 bytes长度的密钥
 *
 * Param:   none
 * Return:  - Error code, possible codes:
 *            AES_ERR_OK    - OK
 *            AES_ERR_VALUE - Parameter of incorrect value.
 * Author:  2012/10/18, hui.pang create this function
 ****************************************************************************/
ERR_T AES_Change_Secret_Key( const UINT8 * p_u8_secret_key )
{    
    UINT8 u8_i = 0;
    
    if( p_u8_secret_key == (void*)0 )
    {
        return AES_ERR_VALUE;
    }

    for( u8_i = 0; u8_i < 16; u8_i++ )
    {
        s_u8_secret_key[u8_i] = p_u8_secret_key[u8_i];
    }

    AES_Init();

    return AES_ERR_OK;
}

/****************************************************************************/
/**
 * Function Name: void AES_Copy_AES_Secret( UINT8 *p_u8_copy_secret )
 * Description: 通过传入的数组拷贝AES Secret，16 bytes
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/26, hui.pang create this function
 ****************************************************************************/
void AES_Copy_AES_Secret( UINT8 *p_u8_copy_secret )
{
    p_u8_copy_secret[0]  = s_u8_secret_key[0];
    p_u8_copy_secret[1]  = s_u8_secret_key[1];
    p_u8_copy_secret[2]  = s_u8_secret_key[2];
    p_u8_copy_secret[3]  = s_u8_secret_key[3];
    p_u8_copy_secret[4]  = s_u8_secret_key[4];
    p_u8_copy_secret[5]  = s_u8_secret_key[5];
    p_u8_copy_secret[6]  = s_u8_secret_key[6];
    p_u8_copy_secret[7]  = s_u8_secret_key[7];
    p_u8_copy_secret[8]  = s_u8_secret_key[8];
    p_u8_copy_secret[9]  = s_u8_secret_key[9];
    p_u8_copy_secret[10] = s_u8_secret_key[10];
    p_u8_copy_secret[11] = s_u8_secret_key[11];
    p_u8_copy_secret[12] = s_u8_secret_key[12];
    p_u8_copy_secret[13] = s_u8_secret_key[13];
    p_u8_copy_secret[14] = s_u8_secret_key[14];
    p_u8_copy_secret[15] = s_u8_secret_key[15];
}

/*****************************************************************************
** End File
*****************************************************************************/
