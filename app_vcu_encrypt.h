/******************************************************************************
 * (C) Copyright 2012 Atech-Automotive
 * FILE NAME:    AES.c ( 2013 / 03/ 19 / 13 / 52 )
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



#ifndef _AES_H_H_
#define _AES_H_H_

#include "system.h"

typedef UINT8 BOOL;
typedef UINT8 ERR_T;

/*****************************************************************************
** #include 
*****************************************************************************/


/*****************************************************************************
** typedef
*****************************************************************************/
typedef enum
{
    AES_ENCRYPT,  /* 加密 */
    AES_DECRYPT,  /* 解密 */
} AES_HANDLE_E;

typedef enum
{
    AES_ERR_OK,      /* OK */
    AES_ERR_VALUE,   /* Parameter of incorrect value. */
} AES_ERR_E;

/*****************************************************************************
** Constant Macro Definition
*****************************************************************************/


/*****************************************************************************
** System Macro Definition
*****************************************************************************/


/*****************************************************************************
** Action Macro Definition
*****************************************************************************/


/*****************************************************************************
** Config Macro Definition
*****************************************************************************/


/*****************************************************************************
** Task Macro Definition
*****************************************************************************/


/*****************************************************************************
** Variables
*****************************************************************************/


/*****************************************************************************
** Constants
*****************************************************************************/


/*****************************************************************************
** Function prototypeseb
*****************************************************************************/

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
                         UINT8 u8_data_len );

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
ERR_T AES_Change_Secret_Key( const UINT8 * p_u8_secret_key );

/****************************************************************************/
/**
 * Function Name: void AES_Copy_AES_Secret( UINT8 *p_u8_copy_secret )
 * Description: 通过传入的数组拷贝AES Secret，16 bytes
 *
 * Param:   none
 * Return:  none
 * Author:  2012/10/26, hui.pang create this function
 ****************************************************************************/
void AES_Copy_AES_Secret( UINT8 *p_u8_copy_secret );

/*****************************************************************************
** other
*****************************************************************************/
// Example
// 上电先执行AES_Change_Secret_Key()，并传入16 bytes的密钥
//UINT8 u8_secret[] = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
//                      0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
//UINT8 u8_data[] = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
//                    0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
//
//AES_Change_Secret_Key( u8_secret );
//
// 加密
//AES_Block_Handle( AES_ENCRYPT, u8_data, 16 );
// 如果原文n bytes，但分配的数组大小必须是((n / 16 + 1)*16) bytes

/****************************************************************************/

#endif	//_AES_H_H_

/*****************************************************************************
** End File
*****************************************************************************/


