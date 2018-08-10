/******************************************************************************
 * (C) Copyright 2012 Atech-Automotive
 * FILE NAME:    AES.c ( 2013 / 03/ 19 / 13 / 52 )
 * DESCRIPTION:  ���㷨ֻ�ṩ��128bit�ļ��ܼ���
 *               Ĭ����Կ: 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88
 *                         0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
 *               AES�㷨�������任����SubBytes���ֽ��������ShiftRows������λ����
 *                        MixColumns���л�������AddRoundKey(����Կ�ӣ�
 *               AES�����㷨����ܲ�ͬ�����������г���AddRoundKey������Կ�ӣ�
 *               �����⣬����Ķ���Ҫ������任����:
 *               InvSubBytes�����ֽ��������InvShiftRows��������λ����
 *               InvMixColumns�����л����� 
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
    AES_ENCRYPT,  /* ���� */
    AES_DECRYPT,  /* ���� */
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
 *              �����p_u8_chiper_data_buf����Ĵ�С������16����������
 *              u8_data_len���Բ���16������������ʵ�ڲ�Ҳ�ǰ�������������ġ�
 *              ����Ҫ������������Ϊ15�������һλ���㡣
 *              
 *              ע��1.�뾡�첻Ҫ���ж���ʹ�øú��������ǿ����뺯����
 *                  2.������ֱ������������������޸ģ��������Ҫ���뱣��Ļ�
 *                    ����ǰ���档
 *                  3.�����������18 bit��������ӦΪ32 bit���������ڲ�������
 *                    ���������Ŀռ䣬���������������Ѿ�Ϊ32 bit��������
 *                    0��䡣
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
 *             ������Կ���뾡�첻Ҫ���ж���ʹ�øú��������ǿ����뺯��
 *             �ڽ���AES�ӽ���֮ǰ��ִ�иú�����������һ��16 bytes���ȵ���Կ
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
 * Description: ͨ����������鿽��AES Secret��16 bytes
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
// �ϵ���ִ��AES_Change_Secret_Key()��������16 bytes����Կ
//UINT8 u8_secret[] = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
//                      0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
//UINT8 u8_data[] = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
//                    0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
//
//AES_Change_Secret_Key( u8_secret );
//
// ����
//AES_Block_Handle( AES_ENCRYPT, u8_data, 16 );
// ���ԭ��n bytes��������������С������((n / 16 + 1)*16) bytes

/****************************************************************************/

#endif	//_AES_H_H_

/*****************************************************************************
** End File
*****************************************************************************/


